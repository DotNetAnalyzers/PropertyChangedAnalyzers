namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ArgumentsWalker : CSharpSyntaxWalker
    {
        private static readonly Pool<ArgumentsWalker> Cache = new Pool<ArgumentsWalker>(
            () => new ArgumentsWalker(),
            x => x.identifierNames.Clear());

        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();

        private ArgumentsWalker()
        {
        }

        public IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        public static Pool<ArgumentsWalker>.Pooled Create(ArgumentListSyntax arguments)
        {
            var pooled = Cache.GetOrCreate();
            pooled.Item.Visit(arguments);
            return pooled;
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            this.identifierNames.Add(node);
            base.VisitIdentifierName(node);
        }

        public bool Contains(IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            foreach (var identifierName in this.IdentifierNames)
            {
                var symbol = semanticModel.GetSymbolSafe(identifierName, cancellationToken) as IParameterSymbol;
                if (parameter.MetadataName == symbol?.MetadataName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}