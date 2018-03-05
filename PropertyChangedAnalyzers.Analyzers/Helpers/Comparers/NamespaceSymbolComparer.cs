namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class NamespaceSymbolComparer : IEqualityComparer<INamespaceSymbol>
    {
        public static readonly NamespaceSymbolComparer Default = new NamespaceSymbolComparer();

        private NamespaceSymbolComparer()
        {
        }

        public static bool Equals(INamespaceSymbol x, INamespaceSymbol y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null ||
                y == null)
            {
                return false;
            }

            return x.MetadataName == y.MetadataName;
        }

        //// ReSharper disable once UnusedMember.Global
        //// ReSharper disable UnusedParameter.Global
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
        [Obsolete("Should only be called with arguments of type INamespaceSymbol.", error: true)]
        public static new bool Equals(object _, object __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        //// ReSharper restore UnusedParameter.Global

        /// <inheritdoc />
        bool IEqualityComparer<INamespaceSymbol>.Equals(INamespaceSymbol x, INamespaceSymbol y) => Equals(x, y);

        /// <inheritdoc />
        public int GetHashCode(INamespaceSymbol obj)
        {
            return obj?.MetadataName.GetHashCode() ?? 0;
        }
    }
}
