namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingInvokerCodeFix))]
    [Shared]
    internal class MissingInvokerCodeFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC007MissingInvoker.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var classDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                ?.FirstAncestorOrSelf<EventFieldDeclarationSyntax>()
                                                ?.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDeclaration != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Add OnPropertyChanged invoker.",
                            cancellationToken => AddInvokerAsync(context.Document, classDeclaration, cancellationToken),
                            "Add OnPropertyChanged invoker."),
                        diagnostic);

                    if (!classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword) &&
                        !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                        NoPropertyShouldNotify(classDeclaration, semanticModel, context.CancellationToken))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Seal class.",
                                _ => Task.FromResult(context.Document.WithSyntaxRoot(
                                        syntaxRoot.ReplaceNode(
                                            classDeclaration,
                                            MakeSealedRewriter.Default.Visit(classDeclaration, classDeclaration)))),
                                "Seal class."),
                            diagnostic);
                    }
                }
            }
        }

        private static async Task<Document> AddInvokerAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var type = editor.SemanticModel.GetDeclaredSymbolSafe(classDeclaration, cancellationToken);
            var usesUnderscoreNames = classDeclaration.UnderscoreFields(editor.SemanticModel, cancellationToken);
            if (type.IsSealed)
            {
                editor.AddMethod(
                    classDeclaration,
                    ParseMethod(
                        @"
private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
{
    this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
}",
                        usesUnderscoreNames));
            }
            else if (type.IsStatic)
            {
                editor.AddMethod(
                    classDeclaration,
                    ParseMethod(
                        @"
private static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
{
    PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
}",
                        usesUnderscoreNames));
            }
            else
            {
                editor.AddMethod(
                    classDeclaration,
                    ParseMethod(
                        @"
protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
{
    this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
}",
                        usesUnderscoreNames));
            }

            return editor.GetChangedDocument();
        }

        private static MethodDeclarationSyntax ParseMethod(string code, bool usesUnderscoreNames)
        {
            if (usesUnderscoreNames)
            {
                code = code.Replace("this.", string.Empty);
            }

            return (MethodDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code)
                                                         .Members
                                                         .Single()
                                                         .WithSimplifiedNames()
                                                         .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                                                         .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                         .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static bool NoPropertyShouldNotify(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property &&
                    Property.ShouldNotify(property, semanticModel, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
