namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GetBackingFieldFix))]
    [Shared]
    internal class GetBackingFieldFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC019GetBackingField.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ExpressionSyntax? expression) &&
                    diagnostic.AdditionalLocations.TrySingle(out var additionalLocation) &&
                    syntaxRoot.TryFindNode(additionalLocation, out ExpressionSyntax? fieldAccess))
                {
                    context.RegisterCodeFix(
                        "Get backing field.",
                        (editor, _) => editor.ReplaceNode(
                            expression,
                            x => fieldAccess.WithTriviaFrom(x)),
                        "Get backing field.",
                        diagnostic);
                }
            }
        }
    }
}
