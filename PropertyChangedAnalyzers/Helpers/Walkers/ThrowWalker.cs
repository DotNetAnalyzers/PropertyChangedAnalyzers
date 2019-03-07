namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ThrowWalker : PooledWalker<ThrowWalker>
    {
        private readonly List<SyntaxToken> throws = new List<SyntaxToken>();

        private ThrowWalker()
        {
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            this.throws.Add(node.ThrowKeyword);
            base.VisitThrowStatement(node);
        }

        public override void VisitThrowExpression(ThrowExpressionSyntax node)
        {
            this.throws.Add(node.ThrowKeyword);
            base.VisitThrowExpression(node);
        }

        internal static ThrowWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new ThrowWalker());

        internal static bool Throws(SyntaxNode scope)
        {
            if (scope == null)
            {
                return false;
            }

            using (var walker = Borrow(scope))
            {
                return walker.throws.Count > 0;
            }
        }

        protected override void Clear()
        {
            this.throws.Clear();
        }
    }
}
