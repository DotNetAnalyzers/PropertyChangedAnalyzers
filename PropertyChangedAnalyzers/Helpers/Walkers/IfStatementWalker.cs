namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IfStatementWalker : PooledWalker<IfStatementWalker>
    {
        private readonly List<IfStatementSyntax> ifStatements = new List<IfStatementSyntax>();

        private IfStatementWalker()
        {
        }

        internal IReadOnlyList<IfStatementSyntax> IfStatements => this.ifStatements;

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            this.ifStatements.Add(node);
            base.VisitIfStatement(node);
        }

        internal static IfStatementWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new IfStatementWalker());

        protected override void Clear()
        {
            this.ifStatements.Clear();
        }
    }
}
