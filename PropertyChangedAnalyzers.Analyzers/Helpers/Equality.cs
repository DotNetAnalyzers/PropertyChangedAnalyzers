namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Equality
    {
        internal static bool HasEqualityOperator(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Enum:
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_DateTime:
                    return true;
            }

            if (type.TypeKind == TypeKind.Enum)
            {
                return true;
            }

            foreach (var op in type.GetMembers("op_Equality"))
            {
                var opMethod = op as IMethodSymbol;
                if (opMethod?.Parameters.Length == 2 &&
                    type.Equals(opMethod.Parameters[0].Type) &&
                    type.Equals(opMethod.Parameters[1].Type))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsOperatorEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as BinaryExpressionSyntax;
            if (equals?.IsKind(SyntaxKind.EqualsExpression) == true)
            {
                return IsLeftAndRight(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsOperatorNotEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as BinaryExpressionSyntax;
            if (equals?.IsKind(SyntaxKind.NotEqualsExpression) == true)
            {
                return IsLeftAndRight(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsObjectEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.ArgumentList?.Arguments.Count == 2 &&
                   invocation.TryGetInvokedSymbol(KnownSymbol.Object.Equals, semanticModel, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsEqualityComparerEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.ArgumentList?.Arguments.Count == 2 &&
                   invocation.TryGetInvokedSymbol(KnownSymbol.EqualityComparerOfT.EqualsMethod, semanticModel, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsStringEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.TryGetInvokedSymbol(KnownSymbol.String.Equals, semanticModel, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsInstanceEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol instance, ISymbol arg)
        {
            var equals = condition as InvocationExpressionSyntax;
            var memberAccess = equals?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(equals, cancellationToken) as IMethodSymbol;
            if (method?.Parameters.Length == 1 &&
                method.Name == "Equals")
            {
                return instance.Equals(semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken)) &&
                       IsArgument(equals, semanticModel, cancellationToken, arg);
            }

            return false;
        }

        internal static bool IsNullableEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as InvocationExpressionSyntax;
            var method = semanticModel.GetSymbolSafe(@equals, cancellationToken) as IMethodSymbol;
            if (method?.Parameters.Length == 2 &&
                method == KnownSymbol.Nullable.Equals)
            {
                return IsArguments(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsReferenceEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.ArgumentList?.Arguments.Count == 2 &&
                   semanticModel.GetSymbolSafe(invocation, cancellationToken) is IMethodSymbol method &&
                   method == KnownSymbol.Object.ReferenceEquals &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool UsesObjectOrNone(ExpressionSyntax condition)
        {
            if (condition is PrefixUnaryExpressionSyntax unary)
            {
                return UsesObjectOrNone(unary.Operand);
            }

            var memberAccess = (condition as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess?.Expression is IdentifierNameSyntax identifierName &&
                !string.Equals(identifierName.Identifier.ValueText, "object", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static bool IsArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            if (invocation?.ArgumentList.Arguments.Count < 2)
            {
                return false;
            }

            return IsArgument(invocation, semanticModel, cancellationToken, first) &&
                   IsArgument(invocation, semanticModel, cancellationToken, other);
        }

        private static bool IsArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol expected)
        {
            if (invocation == null || invocation.ArgumentList.Arguments.Count < 1)
            {
                return false;
            }

            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                var symbol = semanticModel.GetSymbolSafe(argument.Expression, cancellationToken);
                if (expected.Equals(symbol))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsLeftAndRight(BinaryExpressionSyntax equals, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            if (IsIdentifier(equals.Left, semanticModel, cancellationToken, first) &&
                IsIdentifier(equals.Right, semanticModel, cancellationToken, other))
            {
                return true;
            }

            if (IsIdentifier(equals.Left, semanticModel, cancellationToken, other) &&
                IsIdentifier(equals.Right, semanticModel, cancellationToken, first))
            {
                return true;
            }

            return false;
        }

        private static bool IsIdentifier(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol expected)
        {
            if (expected == null)
            {
                return false;
            }

            return expected.Equals(semanticModel.GetSymbolSafe(expression, cancellationToken));
        }
    }
}
