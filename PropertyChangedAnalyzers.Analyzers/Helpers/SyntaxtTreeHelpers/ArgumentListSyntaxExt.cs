namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ArgumentListSyntaxExt
    {
        internal static bool TryGetMatchingArgument(this InvocationExpressionSyntax invocation, IParameterSymbol parameter, out ArgumentSyntax argument)
        {
            return TryGetMatchingArgument(invocation?.ArgumentList, parameter, out argument);
        }

        internal static bool TryGetMatchingArgument(this ObjectCreationExpressionSyntax objectCreation, IParameterSymbol parameter, out ArgumentSyntax argument)
        {
            return TryGetMatchingArgument(objectCreation?.ArgumentList, parameter, out argument);
        }

        internal static bool TryGetMatchingArgument(this ArgumentListSyntax argumentList, IParameterSymbol parameter, out ArgumentSyntax argument)
        {
            argument = null;
            if (argumentList == null ||
                argumentList.Arguments.Count == 0 ||
                parameter == null ||
                parameter.IsParams)
            {
                return false;
            }

            if (argumentList.Arguments.TrySingle(x => x.NameColon?.Name.Identifier.ValueText == parameter.Name, out argument))
            {
                return true;
            }

            return argumentList.Arguments.TryElementAt(parameter.Ordinal, out argument);
        }
    }
}
