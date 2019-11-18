namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ReturnExpressionsWalker : PooledWalker<ReturnExpressionsWalker>
    {
        private readonly List<ExpressionSyntax> returnValues = new List<ExpressionSyntax>();

        private ReturnExpressionsWalker()
        {
        }

        internal IReadOnlyList<ExpressionSyntax> ReturnValues => this.returnValues;

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            this.returnValues.Add(node.Expression);
            base.VisitReturnStatement(node);
        }

        public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            if (!node.TryFirstAncestor<ConstructorDeclarationSyntax>(out _))
            {
                this.returnValues.Add(node.Expression);
                base.VisitArrowExpressionClause(node);
            }
        }

        internal static ReturnExpressionsWalker Empty() => Borrow(() => new ReturnExpressionsWalker());

        internal static ReturnExpressionsWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new ReturnExpressionsWalker());

        internal static bool TryGetSingle(SyntaxNode node, [NotNullWhen(true)] out ExpressionSyntax? returnValue)
        {
            using var walker = Borrow(node);
            return walker.returnValues.TrySingle(out returnValue);
        }

        protected override void Clear()
        {
            this.returnValues.Clear();
        }
    }
}
