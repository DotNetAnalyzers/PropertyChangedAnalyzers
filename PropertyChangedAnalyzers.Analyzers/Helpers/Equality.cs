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
            return condition is BinaryExpressionSyntax binary &&
                   binary.IsKind(SyntaxKind.EqualsExpression) &&
                   IsLeftAndRight(binary, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsOperatorNotEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is BinaryExpressionSyntax binary &&
                   binary.IsKind(SyntaxKind.NotEqualsExpression) &&
                   IsLeftAndRight(binary, semanticModel, cancellationToken, first, other);
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
                   invocation.TryGetInvokedSymbol(KnownSymbol.EqualityComparerOfT.EqualsMethod, semanticModel, cancellationToken, out var equalsMethod) &&

                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsStringEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   GetSymbolType(first) == KnownSymbol.String &&
                   GetSymbolType(other) == KnownSymbol.String &&
                   invocation.TryGetInvokedSymbol(KnownSymbol.String.Equals, semanticModel, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsInstanceEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol instance, ISymbol arg)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.ArgumentList != null &&
                   invocation.ArgumentList.Arguments.TrySingle(out var argument) &&
                   TryGetName(argument.Expression, out var argName) &&
                   argName == GetSymbolName(arg) &&
                   invocation.TryGetInvokedMethodName(out var name) &&
                   name == "Equals" &&
                   invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetName(memberAccess, out var instanceName) &&
                   instanceName == GetSymbolName(instance) &&
                   SymbolComparer.Equals(instance, semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken)) &&
                   SymbolComparer.Equals(semanticModel.GetSymbolSafe(argument.Expression, cancellationToken), arg);
        }

        internal static bool IsNullableEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.TryGetInvokedMethodName(out var methodName) &&
                   methodName == "Equals" &&
                   invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetName(memberAccess.Expression, out var className) &&
                   className == "Nullable" &&
                   invocation.ArgumentList?.Arguments.Count == 2 &&
                   IsMatchingNullable(GetSymbolType(first) as INamedTypeSymbol, GetSymbolType(other) as INamedTypeSymbol) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other) &&
                   semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol == KnownSymbol.Nullable.Equals;

            bool IsMatchingNullable(INamedTypeSymbol type1, INamedTypeSymbol type2)
            {
                if (type1 == null ||
                    type2 == null)
                {
                    return false;
                }

                if (type1 == KnownSymbol.NullableOfT &&
                    type2 == KnownSymbol.NullableOfT)
                {
                    return TypeSymbolComparer.Equals(type1.TypeArguments[0], type2.TypeArguments[0]);
                }

                if (type1 == KnownSymbol.NullableOfT)
                {
                    return TypeSymbolComparer.Equals(type1.TypeArguments[0], type2);
                }

                if (type2 == KnownSymbol.NullableOfT)
                {
                    return TypeSymbolComparer.Equals(type2.TypeArguments[0], type1);
                }

                return false;
            }
        }

        internal static bool IsReferenceEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.ArgumentList?.Arguments.Count == 2 &&
                   invocation.TryGetInvokedSymbol(KnownSymbol.Object.ReferenceEquals, semanticModel, cancellationToken, out _) &&
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
            if (invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count < 2)
            {
                return false;
            }

            var e0 = invocation.ArgumentList.Arguments[0].Expression;
            var e1 = invocation.ArgumentList.Arguments[1].Expression;
            if (TryGetName(e0, out var name0) &&
                TryGetName(e1, out var name1))
            {
                var firstName = GetSymbolName(first);
                var otherName = GetSymbolName(other);
                if (name0 == firstName &&
                    name1 == otherName)
                {
                    return SymbolComparer.Equals(semanticModel.GetSymbolSafe(e0, cancellationToken), first) &&
                           SymbolComparer.Equals(semanticModel.GetSymbolSafe(e1, cancellationToken), other);
                }

                if (name0 == otherName &&
                    name1 == firstName)
                {
                    return SymbolComparer.Equals(semanticModel.GetSymbolSafe(e0, cancellationToken), other) &&
                           SymbolComparer.Equals(semanticModel.GetSymbolSafe(e1, cancellationToken), first);
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
            if (expression == null ||
                expected == null)
            {
                return false;
            }

            if (TryGetName(expression, out var name) &&
                name != GetSymbolName(expected))
            {
                return false;
            }

            return expected.Equals(semanticModel.GetSymbolSafe(expression, cancellationToken));
        }

        private static ITypeSymbol GetSymbolType(ISymbol symbol)
        {
            switch (symbol)
            {
                case IEventSymbol @event:
                    return @event.Type;
                case IFieldSymbol field:
                    return field.Type;
                case ILocalSymbol local:
                    return local.Type;
                case IMethodSymbol method:
                    return method.ReturnType;
                case ITypeSymbol type:
                    return type;
                case IParameterSymbol parameter:
                    return parameter.Type;
                case IPropertySymbol property:
                    return property.Type;
                default:
                    return null;
            }
        }

        private static string GetSymbolName(ISymbol symbol)
        {
            switch (symbol)
            {
                case IEventSymbol @event:
                    return @event.Name;
                case IFieldSymbol field:
                    return field.Name;
                case ILocalSymbol local:
                    return local.Name;
                case IMethodSymbol method:
                    return method.Name;
                case IParameterSymbol parameter:
                    return parameter.Name;
                case IPropertySymbol property:
                    return property.Name;
                default:
                    return null;
            }
        }

        private static bool TryGetName(ExpressionSyntax expression, out string name)
        {
            name = null;
            if (expression is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                name = memberAccess.Name.Identifier.ValueText;
            }

            return name != null;
        }
    }
}
