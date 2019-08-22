namespace PropertyChangedAnalyzers
{
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;

    internal static class Messages
    {
        internal static string Signature(this IMethodSymbol method)
        {
            if (method.IsGenericMethod &&
                method.TypeParameters.TrySingle(out var typeParameter))
            {
                return $"{method.Name}<{typeParameter.Name}>({string.Join(", ", method.Parameters.Select(x => $"{x} {x.Name}"))})";
            }

            return $"{method.Name}({string.Join(", ", method.Parameters.Select(x => $"{x} {x.Name}"))})";
        }
    }
}
