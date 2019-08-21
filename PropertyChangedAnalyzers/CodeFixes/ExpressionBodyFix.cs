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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExpressionBodyFix))]
    [Shared]
    internal class ExpressionBodyFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC020PreferExpressionBodyAccessor.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out AccessorDeclarationSyntax accessor) &&
                    accessor.Body is BlockSyntax block &&
                    block.Statements.TrySingle(out var statement) &&
                    TryGetExpression(statement, out var expression))
                {
                    context.RegisterCodeFix(
                        "To expression body.",
                        (e, cancellationToken) => e.ReplaceNode(
                            accessor,
                            x => x.AsExpressionBody(expression.WithoutTrivia())),
                        nameof(ExpressionBodyFix),
                        diagnostic);
                }
            }
        }

        private static bool TryGetExpression(StatementSyntax statement, out ExpressionSyntax expression)
        {
            switch (statement)
            {
                case ReturnStatementSyntax returnStatement:
                    expression = returnStatement.Expression;
                    break;
                case ExpressionStatementSyntax expressionStatement:
                    expression = expressionStatement.Expression;
                    break;
                default:
                    expression = null;
                    break;
            }

            return expression != null;
        }
    }
}
