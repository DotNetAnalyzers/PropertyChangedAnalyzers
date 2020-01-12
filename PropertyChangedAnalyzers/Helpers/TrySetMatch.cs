namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal struct TrySetMatch
    {
        internal readonly AnalysisResult AnalysisResult;

        internal readonly IParameterSymbol Field;

        internal readonly IParameterSymbol Value;

        internal readonly IParameterSymbol Name;

        internal TrySetMatch(AnalysisResult analysisResult, IParameterSymbol field, IParameterSymbol value, IParameterSymbol name)
        {
            this.AnalysisResult = analysisResult;
            this.Field = field;
            this.Value = value;
            this.Name = name;
        }
    }
}
