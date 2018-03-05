namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;

    internal sealed class PooledHashSet<T> : IDisposable
    {
        private static readonly ConcurrentQueue<PooledHashSet<T>> Cache = new ConcurrentQueue<PooledHashSet<T>>();
        private readonly HashSet<T> inner = new HashSet<T>(GetComparer());

        private int refCount;

        private PooledHashSet()
        {
        }

        public bool Add(T item)
        {
            this.ThrowIfDisposed();
            return this.inner.Add(item);
        }

        public void Dispose()
        {
            this.refCount--;
            Debug.Assert(this.refCount >= 0, "refCount>= 0");
            if (this.refCount == 0)
            {
                this.inner.Clear();
                Cache.Enqueue(this);
            }
        }

        internal static PooledHashSet<T> Borrow()
        {
            if (!Cache.TryDequeue(out var set))
            {
                set = new PooledHashSet<T>();
            }

            set.refCount = 1;
            return set;
        }

        internal static PooledHashSet<T> Borrow(PooledHashSet<T> set)
        {
            if (set == null)
            {
                return Borrow();
            }

            set.refCount++;
            return set;
        }

        private static IEqualityComparer<T> GetComparer()
        {
            if (typeof(T) == typeof(IAssemblySymbol))
            {
                return (IEqualityComparer<T>)AssemblySymbolComparer.Default;
            }

            if (typeof(T) == typeof(IEventSymbol))
            {
                return (IEqualityComparer<T>)EventSymbolComparer.Default;
            }

            if (typeof(T) == typeof(IFieldSymbol))
            {
                return (IEqualityComparer<T>)FieldSymbolComparer.Default;
            }

            if (typeof(T) == typeof(ILocalSymbol))
            {
                return (IEqualityComparer<T>)LocalSymbolComparer.Default;
            }

            if (typeof(T) == typeof(IMethodSymbol))
            {
                return (IEqualityComparer<T>)MethodSymbolComparer.Default;
            }

            if (typeof(T) == typeof(INamedTypeSymbol))
            {
                return (IEqualityComparer<T>)NamedTypeSymbolComparer.Default;
            }

            if (typeof(T) == typeof(INamespaceSymbol))
            {
                return (IEqualityComparer<T>)NamespaceSymbolComparer.Default;
            }

            if (typeof(T) == typeof(IParameterSymbol))
            {
                return (IEqualityComparer<T>)ParameterSymbolComparer.Default;
            }

            if (typeof(T) == typeof(IPropertySymbol))
            {
                return (IEqualityComparer<T>)PropertySymbolComparer.Default;
            }

            if (typeof(T) == typeof(ISymbol))
            {
                return (IEqualityComparer<T>)SymbolComparer.Default;
            }

            if (typeof(T) == typeof(ITypeSymbol))
            {
                return (IEqualityComparer<T>)TypeSymbolComparer.Default;
            }

            return EqualityComparer<T>.Default;
        }

        [Conditional("DEBUG")]
        private void ThrowIfDisposed()
        {
            if (this.refCount <= 0)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
