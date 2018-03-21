namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class ParameterSymbolExt
    {
        internal static bool IsCallerMemberName(this IParameterSymbol parameter)
        {
            if (parameter.HasExplicitDefaultValue &&
                parameter.Type == KnownSymbol.String)
            {
                foreach (var attribute in parameter.GetAttributes())
                {
                    if (attribute.AttributeClass == KnownSymbol.CallerMemberNameAttribute)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
