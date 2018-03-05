namespace PropertyChangedAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ArgumentSyntaxExt
    {
        internal static bool TryGetStringValue(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (argument?.Expression == null)
            {
                return false;
            }

            if (argument.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return true;
            }

            if (argument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var cv = semanticModel.GetConstantValueSafe(argument.Expression, cancellationToken);
                if (cv.HasValue && cv.Value is string)
                {
                    result = (string)cv.Value;
                    return true;
                }
            }

            if (argument.Expression.IsNameOf())
            {
                var cv = semanticModel.GetConstantValueSafe(argument.Expression, cancellationToken);
                if (cv.HasValue && cv.Value is string)
                {
                    result = (string)cv.Value;
                    return true;
                }
            }

            if (argument.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.ValueText == "Empty" &&
                ((memberAccess.Expression is PredefinedTypeSyntax predefinedType &&
                  predefinedType.Keyword.Text == "string") ||
                  (memberAccess.Expression is IdentifierNameSyntax identifierName &&
                   identifierName.Identifier.ValueText == "String")))
            {
                if (semanticModel.GetSymbolSafe(argument.Expression, cancellationToken) is IFieldSymbol field &&
                    field == KnownSymbol.String.Empty)
                {
                    result = string.Empty;
                    return true;
                }
            }

            return false;
        }

        private static bool IsNameOf(this ExpressionSyntax expression)
        {
            return (expression as InvocationExpressionSyntax)?.IsNameOf() == true;
        }
    }
}
