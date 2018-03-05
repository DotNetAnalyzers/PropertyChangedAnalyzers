namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class SymbolComparer : IEqualityComparer<ISymbol>
    {
        public static readonly SymbolComparer Default = new SymbolComparer();

        private SymbolComparer()
        {
        }

        public static bool Equals(ISymbol x, ISymbol y)
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

            if (x is IEventSymbol xEvent &&
                y is IEventSymbol yEvent)
            {
                return EventSymbolComparer.Equals(xEvent, yEvent);
            }

            if (x is IFieldSymbol xField &&
                y is IFieldSymbol yField)
            {
                return FieldSymbolComparer.Equals(xField, yField);
            }

            if (x is ILocalSymbol xLocal &&
                y is ILocalSymbol yLocal)
            {
                return LocalSymbolComparer.Equals(xLocal, yLocal);
            }

            if (x is IMethodSymbol xMethod &&
                y is IMethodSymbol yMethod)
            {
                return MethodSymbolComparer.Equals(xMethod, yMethod);
            }

            if (x is INamedTypeSymbol xNamedType &&
                y is INamedTypeSymbol yNamedType)
            {
                return NamedTypeSymbolComparer.Equals(xNamedType, yNamedType);
            }

            if (x is INamespaceSymbol xNamespace &&
                y is INamespaceSymbol yNamespace)
            {
                return NamespaceSymbolComparer.Equals(xNamespace, yNamespace);
            }

            if (x is IParameterSymbol xParameter &&
                y is IParameterSymbol yParameter)
            {
                return ParameterSymbolComparer.Equals(xParameter, yParameter);
            }

            if (x is IPropertySymbol xProperty &&
                y is IPropertySymbol yProperty)
            {
                return PropertySymbolComparer.Equals(xProperty, yProperty);
            }

            if (x is ITypeSymbol xType &&
                y is ITypeSymbol yType)
            {
                return TypeSymbolComparer.Equals(xType, yType);
            }

            return x.Equals(y);
        }

        //// ReSharper disable UnusedMember.Global
        //// ReSharper disable UnusedParameter.Global
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static new bool Equals(object _, object __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(IEventSymbol _, IEventSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(IFieldSymbol _, IFieldSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(ILocalSymbol _, ILocalSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(IMethodSymbol _, IMethodSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(INamedTypeSymbol _, INamedTypeSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(INamespaceSymbol _, INamespaceSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(IParameterSymbol _, IParameterSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(IPropertySymbol _, IPropertySymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");

        [Obsolete("Should only be called with arguments of type ISymbol.", error: true)]
        public static bool Equals(ITypeSymbol _, ITypeSymbol __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        //// ReSharper restore UnusedMember.Global
        //// ReSharper restore UnusedParameter.Global

        /// <inheritdoc/>
        bool IEqualityComparer<ISymbol>.Equals(ISymbol x, ISymbol y) => Equals(x, y);

        /// <inheritdoc/>
        public int GetHashCode(ISymbol obj)
        {
            if (obj is ITypeSymbol typeSymbol)
            {
                return TypeSymbolComparer.GetHashCode(typeSymbol);
            }

            return obj?.MetadataName.GetHashCode() ?? 0;
        }
    }
}
