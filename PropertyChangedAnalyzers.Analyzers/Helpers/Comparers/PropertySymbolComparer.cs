namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class PropertySymbolComparer : IEqualityComparer<IPropertySymbol>
    {
        public static readonly PropertySymbolComparer Default = new PropertySymbolComparer();

        private PropertySymbolComparer()
        {
        }

        public static bool Equals(IPropertySymbol x, IPropertySymbol y)
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

            return x.MetadataName == y.MetadataName &&
                   NamedTypeSymbolComparer.Equals(x.ContainingType, y.ContainingType);
        }

        //// ReSharper disable once UnusedMember.Global
        //// ReSharper disable UnusedParameter.Global
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
        [Obsolete("Should only be called with arguments of type IPropertySymbol.", error: true)]
        public static new bool Equals(object _, object __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        //// ReSharper restore UnusedParameter.Global

        /// <inheritdoc />
        bool IEqualityComparer<IPropertySymbol>.Equals(IPropertySymbol x, IPropertySymbol y) => Equals(x, y);

        /// <inheritdoc />
        public int GetHashCode(IPropertySymbol obj)
        {
            return obj?.MetadataName.GetHashCode() ?? 0;
        }
    }
}
