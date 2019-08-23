namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddInvokerFix))]
    [Shared]
    internal class AddInvokerFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC007MissingInvoker.Id);

        protected override DocumentEditorFixAllProvider FixAllProvider() => DocumentEditorFixAllProvider.Project;

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ClassDeclarationSyntax classDeclaration))
                {
                    context.RegisterCodeFix(
                        "Add OnPropertyChanged()",
                        (editor, cancellationToken) =>
                            AddOnPropertyChanged(editor, classDeclaration, cancellationToken),
                        "Add OnPropertyChanged()",
                        diagnostic);

                    if (!classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword) &&
                        !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                        NoPropertyShouldNotify(classDeclaration, semanticModel, context.CancellationToken))
                    {
                        context.RegisterCodeFix(
                            "Seal class.",
                            (editor, _) => editor.Seal(classDeclaration),
                            "Seal class.",
                            diagnostic);
                    }
                }
            }
        }

        private static void AddOnPropertyChanged(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var type = editor.SemanticModel.GetDeclaredSymbolSafe(classDeclaration, cancellationToken);
            var underscoreFields = editor.SemanticModel.UnderscoreFields();
            if (type.IsSealed)
            {
                _ = editor.AddMethod(
                    classDeclaration,
                    ParseMethod(
                        @"
private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
{
    this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
}",
                        underscoreFields));
            }
            else if (type.IsStatic)
            {
                _ = editor.AddMethod(
                    classDeclaration,
                    ParseMethod(
                        @"
private static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
{
    PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
}",
                        underscoreFields));
            }
            else
            {
                _ = editor.AddMethod(
                    classDeclaration,
                    ParseMethod(
                        @"
protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
{
    this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
}",
                        underscoreFields));
            }
        }

        private static MethodDeclarationSyntax ParseMethod(string code, CodeStyleResult underscoreFields)
        {
            if (underscoreFields == CodeStyleResult.Yes)
            {
                code = code.Replace("this.", string.Empty);
            }

            return Parse.MethodDeclaration(code)
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
