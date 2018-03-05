namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class TypeSymbolComparer : IEqualityComparer<ITypeSymbol>
    {
        public static readonly TypeSymbolComparer Default = new TypeSymbolComparer();

        private TypeSymbolComparer()
        {
        }

        public static bool Equals(ITypeSymbol x, ITypeSymbol y)
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

            if (x.TypeKind != y.TypeKind)
            {
                return false;
            }

            if (x is INamedTypeSymbol xNamedType &&
                y is INamedTypeSymbol yNamedType)
            {
                return NamedTypeSymbolComparer.Equals(xNamedType, yNamedType);
            }

            if (x.TypeKind == TypeKind.TypeParameter)
            {
                return x.MetadataName == y.MetadataName &&
                       SymbolComparer.Equals(x.ContainingSymbol, y.ContainingSymbol);
            }

            return x.MetadataName == y.MetadataName &&
                   Equals(x.ContainingType, y.ContainingType) &&
                   NamespaceSymbolComparer.Equals(x.ContainingNamespace, y.ContainingNamespace);
        }

        //// ReSharper disable once UnusedMember.Global
        //// ReSharper disable UnusedParameter.Global
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
        [Obsolete("Should only be called with arguments of type ITypeSymbol.", error: true)]
        public static new bool Equals(object _, object __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        //// ReSharper restore UnusedParameter.Global

        public static int GetHashCode(ITypeSymbol obj)
        {
            if (obj == null)
            {
                return 0;
            }

            if (obj.TypeKind == TypeKind.TypeParameter)
            {
                return 1;
            }

            return obj.MetadataName.GetHashCode();
        }

        /// <inheritdoc />
        bool IEqualityComparer<ITypeSymbol>.Equals(ITypeSymbol x, ITypeSymbol y) => Equals(x, y);

        /// <inheritdoc />
        int IEqualityComparer<ITypeSymbol>.GetHashCode(ITypeSymbol obj)
        {
            return GetHashCode(obj);
        }
    }
}
