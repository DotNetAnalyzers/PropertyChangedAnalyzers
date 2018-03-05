namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class ParameterSymbolComparer : IEqualityComparer<IParameterSymbol>
    {
        public static readonly ParameterSymbolComparer Default = new ParameterSymbolComparer();

        private ParameterSymbolComparer()
        {
        }

        public static bool Equals(IParameterSymbol x, IParameterSymbol y)
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
                   SymbolComparer.Equals(x.ContainingSymbol, y.ContainingSymbol);
        }

        //// ReSharper disable once UnusedMember.Global
        //// ReSharper disable UnusedParameter.Global
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
        [Obsolete("Should only be called with arguments of type IParameterSymbol.", error: true)]
        public static new bool Equals(object _, object __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        //// ReSharper restore UnusedParameter.Global

        /// <inheritdoc />
        bool IEqualityComparer<IParameterSymbol>.Equals(IParameterSymbol x, IParameterSymbol y) => Equals(x, y);

        /// <inheritdoc />
        public int GetHashCode(IParameterSymbol obj)
        {
            return obj?.MetadataName.GetHashCode() ?? 0;
        }
    }
}
