namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChangedEventArgs
    {
        internal static bool TryGetPropertyName(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out string propertyName)
        {
            return TryGetCreation(expression, semanticModel, cancellationToken, out _, out propertyName) ||
                   TryGetCached(expression, semanticModel, cancellationToken, out _, out propertyName);
        }

        private static bool TryGetCached(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string propertyName)
        {
            var cached = semanticModel.GetSymbolSafe(expression, cancellationToken);
            if (cached is IFieldSymbol field &&
                field.TryGetSingleDeclaration(cancellationToken, out var fieldDeclaration) &&
                fieldDeclaration.Declaration.Variables.TryLast(out var variable) &&
                variable.Initializer is EqualsValueClauseSyntax initializer)
            {
                return TryGetCreation(initializer.Value, semanticModel, cancellationToken, out nameArg, out propertyName);
            }

            if (cached is IPropertySymbol property &&
                property.TrySingleDeclaration(cancellationToken, out var propertyDeclaration))
            {
                return TryGetCreation(propertyDeclaration.Initializer?.Value, semanticModel, cancellationToken, out nameArg, out propertyName);
            }

            nameArg = null;
            propertyName = null;
            return false;
        }

        private static bool TryGetCreation(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string propertyName)
        {
            nameArg = null;
            propertyName = null;
            if (expression is ObjectCreationExpressionSyntax objectCreation &&
                objectCreation.ArgumentList is ArgumentListSyntax argumentList &&
                objectCreation.Type == KnownSymbol.PropertyChangedEventArgs &&
                argumentList.Arguments.TrySingle(out nameArg))
            {
                return nameArg.TryGetStringValue(semanticModel, cancellationToken, out propertyName);
            }

            return false;
        }
    }
}
