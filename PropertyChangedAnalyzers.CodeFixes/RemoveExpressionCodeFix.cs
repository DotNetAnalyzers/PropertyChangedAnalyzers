namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveExpressionCodeFix))]
    [Shared]
    internal class RemoveExpressionCodeFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC012DontUseExpression.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document
                                             .GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            var underscoreFields = semanticModel.UnderscoreFields();
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
                if (PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, context.CancellationToken, out var invoker))
                {
                    context.RegisterCodeFix(
                        "Use overload that does not use expression.",
                        (editor, cancellationToken) => RemoveExpression(editor, argument, invoker, underscoreFields, cancellationToken),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static void RemoveExpression(DocumentEditor editor, ArgumentSyntax argument, IMethodSymbol invoker, bool usesUnderscoreNames, CancellationToken cancellationToken)
        {
            var invocation = argument.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (PropertyChanged.TryGetInvokedPropertyChangedName(invocation, editor.SemanticModel, cancellationToken, out var name) == AnalysisResult.Yes)
            {
                var property = editor.SemanticModel.GetDeclaredSymbolSafe(argument.FirstAncestorOrSelf<PropertyDeclarationSyntax>(), cancellationToken);
                if (property?.Name == name)
                {
                    editor.ReplaceNode(
                        invocation,
                        Trivia.WithTrailingElasticLineFeed(
                                         SyntaxFactory.ParseExpression(Snippet.OnPropertyChanged(invoker, property?.Name, usesUnderscoreNames).TrimEnd(';'))
                                                      .WithSimplifiedNames()
                                                      .WithLeadingElasticLineFeed())
                                     .WithAdditionalAnnotations(Formatter.Annotation));
                }
                else
                {
                    editor.ReplaceNode(
                        invocation,
                        Trivia.WithTrailingElasticLineFeed(
                                         SyntaxFactory.ParseExpression(Snippet.OnOtherPropertyChanged(invoker, name, usesUnderscoreNames).TrimEnd(';'))
                                                      .WithSimplifiedNames()
                                                      .WithLeadingElasticLineFeed())
                                     .WithAdditionalAnnotations(Formatter.Annotation));
                }
            }
        }
    }
}
