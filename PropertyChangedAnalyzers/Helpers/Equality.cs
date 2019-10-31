namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Obsolete("Use Gu.Roslyn.Extensions")]
    internal static class Equality
    {
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
                   semanticModel.TryGetSymbol(invocation, KnownSymbol.Object.Equals, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsEqualityComparerEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.ArgumentList?.Arguments.Count == 2 &&
                   invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetName(memberAccess.Expression, out var instance) &&
                   !string.Equals(instance, "object", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(instance, "Nullable", StringComparison.OrdinalIgnoreCase) &&
                   instance != GetSymbolName(first) &&
                   instance != GetSymbolName(other) &&
                   semanticModel.TryGetSymbol(invocation, KnownSymbol.EqualityComparerOfT.EqualsMethod, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsStringEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetName(memberAccess.Expression, out var className) &&
                   string.Equals(className, "string", StringComparison.OrdinalIgnoreCase) &&
                   GetSymbolType(first) == KnownSymbol.String &&
                   GetSymbolType(other) == KnownSymbol.String &&
                   semanticModel.TryGetSymbol(invocation, KnownSymbol.String.Equals, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
        }

        internal static bool IsInstanceEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol instance, ISymbol arg)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.ArgumentList != null &&
                   invocation.ArgumentList.Arguments.TrySingle(out var argument) &&
                   TryGetName(argument.Expression, out var argName) &&
                   argName == GetSymbolName(arg) &&
                   invocation.TryGetMethodName(out var name) &&
                   name == "Equals" &&
                   invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetName(memberAccess.Expression, out var instanceName) &&
                   instanceName == GetSymbolName(instance) &&
                   SymbolComparer.Equals(instance, semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken)) &&
                   SymbolComparer.Equals(semanticModel.GetSymbolSafe(argument.Expression, cancellationToken), arg);
        }

        internal static bool IsNullableEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            return condition is InvocationExpressionSyntax invocation &&
                   invocation.TryGetMethodName(out var methodName) &&
                   methodName == "Equals" &&
                   invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   TryGetName(memberAccess.Expression, out var className) &&
                   className == "Nullable" &&
                   invocation.ArgumentList?.Arguments.Count == 2 &&
                   IsMatchingNullable(GetSymbolType(first) as INamedTypeSymbol, GetSymbolType(other) as INamedTypeSymbol) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other) &&
                   semanticModel.GetSymbolSafe(invocation, cancellationToken) == KnownSymbol.Nullable.Equals;

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
                   semanticModel.TryGetSymbol(invocation, KnownSymbol.Object.ReferenceEquals, cancellationToken, out _) &&
                   IsArguments(invocation, semanticModel, cancellationToken, first, other);
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

        private static ITypeSymbol? GetSymbolType(ISymbol symbol)
        {
            return symbol switch
            {
                IEventSymbol @event => @event.Type,
                IFieldSymbol field => field.Type,
                ILocalSymbol local => local.Type,
                IMethodSymbol method => method.ReturnType,
                ITypeSymbol type => type,
                IParameterSymbol parameter => parameter.Type,
                IPropertySymbol property => property.Type,
                _ => null,
            };
        }

        private static string? GetSymbolName(ISymbol symbol)
        {
            return symbol switch
            {
                IEventSymbol @event => @event.Name,
                IFieldSymbol field => field.Name,
                ILocalSymbol local => local.Name,
                IMethodSymbol method => method.Name,
                IParameterSymbol parameter => parameter.Name,
                IPropertySymbol property => property.Name,
                _ => null,
            };
        }

        private static bool TryGetName(ExpressionSyntax expression, out string name)
        {
            name = null;
            if (expression is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
            }
            else if (expression is PredefinedTypeSyntax predefinedType)
            {
                name = predefinedType.Keyword.ValueText;
            }
            else if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                name = memberAccess.Name.Identifier.ValueText;
            }

            return name != null;
        }
    }
}
