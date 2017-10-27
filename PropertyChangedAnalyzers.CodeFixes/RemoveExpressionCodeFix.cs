namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveExpressionCodeFix))]
    [Shared]
    internal class RemoveExpressionCodeFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC012DontUseExpression.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document
                                             .GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            var usesUnderscoreNames = syntaxRoot.UsesUnderscore(semanticModel, context.CancellationToken);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var argument = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                         .FirstAncestorOrSelf<ArgumentSyntax>();
                var type = semanticModel.GetDeclaredSymbolSafe(argument?.FirstAncestorOrSelf<ClassDeclarationSyntax>(), context.CancellationToken);
                if (PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out var invoker))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use overload that does not use expression.",
                            cancellationToken => RemoveExpressionAsync(context.Document, argument, invoker, usesUnderscoreNames, cancellationToken),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static async Task<Document> RemoveExpressionAsync(Document document, ArgumentSyntax argument, IMethodSymbol invoker, bool usesUnderscoreNames, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var invocation = argument.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (PropertyChanged.TryGetInvokedPropertyChangedName(invocation, editor.SemanticModel, cancellationToken, out _, out var name) == AnalysisResult.Yes)
            {
                var propertySymbol = editor.SemanticModel.GetDeclaredSymbolSafe(argument.FirstAncestorOrSelf<PropertyDeclarationSyntax>(), cancellationToken);
                if (propertySymbol?.Name == name)
                {
                    editor.ReplaceNode(
                        invocation,
                        SyntaxFactory.ParseExpression(Snippet.OnPropertyChanged(invoker, propertySymbol, usesUnderscoreNames).TrimEnd(';'))
                                     .WithSimplifiedNames()
                                     .WithLeadingElasticLineFeed()
                                     .WithTrailingElasticLineFeed()
                                     .WithAdditionalAnnotations(Formatter.Annotation));
                }
                else
                {
                    editor.ReplaceNode(
                        invocation,
                        SyntaxFactory.ParseExpression(Snippet.OnOtherPropertyChanged(invoker, name, usesUnderscoreNames).TrimEnd(';'))
                                     .WithSimplifiedNames()
                                     .WithLeadingElasticLineFeed()
                                     .WithTrailingElasticLineFeed()
                                     .WithAdditionalAnnotations(Formatter.Annotation));
                }
            }

            return editor.GetChangedDocument();
        }
    }
}