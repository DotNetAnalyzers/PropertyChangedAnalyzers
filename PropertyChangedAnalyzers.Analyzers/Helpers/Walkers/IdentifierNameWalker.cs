namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IdentifierNameWalker : CSharpSyntaxWalker, IDisposable
    {
        private static readonly ConcurrentQueue<IdentifierNameWalker> Cache = new ConcurrentQueue<IdentifierNameWalker>();

        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();
        private int refCount;

        private IdentifierNameWalker()
        {
        }

        public IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        public static IdentifierNameWalker Borrow(SyntaxNode node)
        {
            if (!Cache.TryDequeue(out var walker))
            {
                walker = new IdentifierNameWalker();
            }

            walker.refCount = 0;
            walker.Visit(node);
            return walker;
        }

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

        public void Dispose()
        {
            this.refCount--;
            if (this.refCount == 0)
            {
                this.identifierNames.Clear();
                Cache.Enqueue(this);
            }
        }

        [Conditional("DEBUG")]
        private void ThrowIfDisposed()
        {
            if (this.refCount == 0)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}