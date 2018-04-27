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

        public IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        public static IdentifierNameWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new IdentifierNameWalker());

        public static bool Contains(SyntaxNode node, IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using (var walker = Borrow(node))
            {
                return walker.Contains(parameter, semanticModel, cancellationToken);
            }
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            this.identifierNames.Add(node);
            base.VisitIdentifierName(node);
        }

        public bool Contains(IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            if (parameter is null)
            {
                return false;
            }

            foreach (var identifierName in this.identifierNames)
            {
                if (parameter.MetadataName == identifierName.Identifier.ValueText &&
                    SemanticModelExt.GetSymbolSafe(semanticModel, identifierName, cancellationToken) is IParameterSymbol)
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
