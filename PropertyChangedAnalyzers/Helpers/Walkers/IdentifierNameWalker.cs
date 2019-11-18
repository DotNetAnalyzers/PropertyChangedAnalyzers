namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IdentifierNameWalker : PooledWalker<IdentifierNameWalker>
    {
        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();

        private IdentifierNameWalker()
        {
        }

        internal IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            this.identifierNames.Add(node);
            base.VisitIdentifierName(node);
        }

        internal static IdentifierNameWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new IdentifierNameWalker());

        internal static bool Contains(SyntaxNode node, IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using var walker = Borrow(node);
            foreach (var identifierName in walker.identifierNames)
            {
                if (parameter.MetadataName == identifierName.Identifier.ValueText &&
                    semanticModel.TryGetSymbol(identifierName, cancellationToken, out IParameterSymbol _))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Clear()
        {
            this.identifierNames.Clear();
        }
    }
}
