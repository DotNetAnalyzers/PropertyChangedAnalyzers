namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
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
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out AccessorDeclarationSyntax? accessor) &&
                    accessor.Body is { Statements: { Count: 1 } statements } &&
                    GetExpression(statements[0]) is { } expression)
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

        private static ExpressionSyntax? GetExpression(StatementSyntax statement)
        {
            return statement switch
            {
                ReturnStatementSyntax { Expression: { } expression } => expression,
                ExpressionStatementSyntax { Expression: { } expression } => expression,
                _ => null,
            };
        }
    }
}
