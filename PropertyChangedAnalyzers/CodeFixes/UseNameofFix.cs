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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofFix))]
    [Shared]
    internal class UseNameofFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC013UseNameof.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is ArgumentSyntax { Expression: LiteralExpressionSyntax literal } argument &&
                    semanticModel is { } &&
                    semanticModel.LookupSymbols(argument.SpanStart, name: literal.Token.ValueText).TryFirst(out var member))
                {
                    context.RegisterCodeFix(
                        "Use nameof",
                        async (editor, cancellationToken) =>
                        {
                            var replacement = await editor.SymbolAccessAsync(member, literal, cancellationToken)
                                                          .ConfigureAwait(false);
                            _ = editor.ReplaceNode(
                                literal,
                                x => InpcFactory.Nameof(replacement).WithTriviaFrom(x));
                        },
                        nameof(UseNameofFix),
                        diagnostic);
                }
            }
        }
    }
}
