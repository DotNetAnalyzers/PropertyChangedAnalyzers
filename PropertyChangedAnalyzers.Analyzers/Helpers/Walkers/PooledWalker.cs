namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal abstract class PooledWalker : CSharpSyntaxWalker, IDisposable
    {
        private static readonly ConcurrentQueue<PooledWalker> Cache = new ConcurrentQueue<PooledWalker>();
        private int refCount;

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected static T Borrow<T>(SyntaxNode node, Func<T> create)
            where T : PooledWalker
        {
            if (!Cache.TryDequeue(out var walker))
            {
                walker = create();
            }

            walker.refCount = 0;
            walker.Visit(node);
            return (T)walker;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.refCount--;
                Debug.Assert(this.refCount >= 0, "refCount>= 0");
                if (this.refCount == 0)
                {
                    this.Clear();
                    Cache.Enqueue(this);
                }
            }
        }

        protected abstract void Clear();

        [Conditional("DEBUG")]
        protected void ThrowIfDisposed()
        {
            if (this.refCount <= 0)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}