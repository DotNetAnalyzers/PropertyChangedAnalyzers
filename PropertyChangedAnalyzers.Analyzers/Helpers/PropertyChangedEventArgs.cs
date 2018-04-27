namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChangedEventArgs
    {
        public static bool IsCreatedWith(ExpressionSyntax expression, IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (TryGetCached(expression, semanticModel, cancellationToken, out var argument))
            {
                return argument.Expression is IdentifierNameSyntax identifierName &&
                       identifierName.Identifier.ValueText == parameter.Name;
            }

            if (expression is ObjectCreationExpressionSyntax)
            {
                return parameter.Type == KnownSymbol.String &&
                       TryGetCreation(expression, out argument) &&
                       argument.Expression is IdentifierNameSyntax identifierName &&
                       identifierName.Identifier.ValueText == parameter.Name;
            }

            return false;
        }

        internal static bool TryGetPropertyName(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out string propertyName)
        {
            if (TryGetCreation(expression, out var nameArgument) ||
                TryGetCached(expression, semanticModel, cancellationToken, out nameArgument))
            {
                return nameArgument.TryGetStringValue(semanticModel, cancellationToken, out propertyName);
            }

            propertyName = null;
            return false;
        }

        internal static bool TryGetPropertyNameArgument(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArgument)
        {
            return TryGetCreation(expression, out nameArgument) ||
                   TryGetCached(expression, semanticModel, cancellationToken, out nameArgument);
        }

        private static bool TryGetCached(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg)
        {
            var cached = semanticModel.GetSymbolSafe(expression, cancellationToken);
            if (cached is IFieldSymbol field &&
                field.TrySingleDeclaration(cancellationToken, out var fieldDeclaration) &&
                fieldDeclaration.Declaration.Variables.TryLast(out var variable) &&
                variable.Initializer is EqualsValueClauseSyntax fieldInitializer)
            {
                return TryGetCreation(fieldInitializer.Value, out nameArg);
            }

            if (cached is IPropertySymbol property &&
                property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
            {
                return TryGetCreation(propertyDeclaration.Initializer?.Value, out nameArg);
            }

            if (cached is ILocalSymbol local &&
                local.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax variableDeclaration) &&
                variableDeclaration.Variables.TryLast(out variable) &&
                variable.Initializer is EqualsValueClauseSyntax initializer)
            {
                return TryGetCreation(initializer.Value, out nameArg);
            }

            nameArg = null;
            return false;
        }

        private static bool TryGetCreation(this ExpressionSyntax expression, out ArgumentSyntax nameArg)
        {
            nameArg = null;
            return expression is ObjectCreationExpressionSyntax objectCreation &&
                   objectCreation.ArgumentList is ArgumentListSyntax argumentList &&
                   objectCreation.Type == KnownSymbol.PropertyChangedEventArgs &&
                   argumentList.Arguments.TrySingle(out nameArg);
        }
    }
}
