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
            return TryGetStringValue(argument?.Expression, out result);

            bool TryGetStringValue(ExpressionSyntax expression, out string text)
            {
                text = null;
                if (expression == null)
                {
                    return false;
                }

                if (expression is LiteralExpressionSyntax literal)
                {
                    switch (literal.Kind())
                    {
                        case SyntaxKind.NullLiteralExpression:
                            return true;
                        case SyntaxKind.StringLiteralExpression:
                            text = literal.Token.ValueText;
                            return true;
                    }
                }

                if (expression.IsNameOf())
                {
                    var cv = semanticModel.GetConstantValueSafe(expression, cancellationToken);
                    if (cv.HasValue &&
                        cv.Value is string)
                    {
                        text = (string)cv.Value;
                        return true;
                    }
                }

                if (expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Name.Identifier.ValueText == "Empty" &&
                    ((memberAccess.Expression is PredefinedTypeSyntax predefinedType &&
                      predefinedType.Keyword.Text == "string") ||
                     (memberAccess.Expression is IdentifierNameSyntax identifierName &&
                      identifierName.Identifier.ValueText == "String")))
                {
                    if (semanticModel.GetSymbolSafe(expression, cancellationToken) is IFieldSymbol field &&
                        field == KnownSymbol.String.Empty)
                    {
                        text = string.Empty;
                        return true;
                    }
                }

                if (expression is CastExpressionSyntax castExpression)
                {
                    return TryGetStringValue(castExpression.Expression, out text);
                }

                return false;
            }
        }

        private static bool IsNameOf(this ExpressionSyntax expression)
        {
            return (expression as InvocationExpressionSyntax)?.IsNameOf() == true;
        }
    }
}
