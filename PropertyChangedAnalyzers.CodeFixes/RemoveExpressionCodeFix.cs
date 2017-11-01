namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
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

        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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
                    context.RegisterDocumentEditorFix(
                        "Use overload that does not use expression.",
                        (editor, cancellationToken) => RemoveExpression(editor, argument, invoker, usesUnderscoreNames, cancellationToken),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static void RemoveExpression(DocumentEditor editor, ArgumentSyntax argument, IMethodSymbol invoker, bool usesUnderscoreNames, CancellationToken cancellationToken)
        {
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
        }
    }
}