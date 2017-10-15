namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IdentifierNameWalker : PooledWalker
    {
        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();

        private IdentifierNameWalker()
        {
        }

        public IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        public static IdentifierNameWalker Borrow(SyntaxNode node) => Borrow(node, () => new IdentifierNameWalker());

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            this.identifierNames.Add(node);
            base.VisitIdentifierName(node);
        }

        public bool Contains(IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            foreach (var identifierName in this.identifierNames)
            {
                var symbol = semanticModel.GetSymbolSafe(identifierName, cancellationToken) as IParameterSymbol;
                if (parameter.MetadataName == symbol?.MetadataName)
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