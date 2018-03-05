namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class EventSymbolComparer : IEqualityComparer<IEventSymbol>
    {
        public static readonly EventSymbolComparer Default = new EventSymbolComparer();

        private EventSymbolComparer()
        {
        }

        public static bool Equals(IEventSymbol x, IEventSymbol y)
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
        [Obsolete("Should only be called with arguments of type IEventSymbol.", error: true)]
        public static new bool Equals(object _, object __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        //// ReSharper restore UnusedParameter.Global

        /// <inheritdoc />
        bool IEqualityComparer<IEventSymbol>.Equals(IEventSymbol x, IEventSymbol y) => Equals(x, y);

        /// <inheritdoc />
        public int GetHashCode(IEventSymbol obj)
        {
            return obj?.MetadataName.GetHashCode() ?? 0;
        }
    }
}
