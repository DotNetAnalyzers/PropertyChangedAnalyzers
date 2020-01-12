namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal struct OnPropertyChangedMatch
    {
        internal readonly AnalysisResult AnalysisResult;
        internal readonly IParameterSymbol Name;

        internal OnPropertyChangedMatch(AnalysisResult analysisResult, IParameterSymbol name)
        {
            this.AnalysisResult = analysisResult;
            this.Name = name;
        }
    }
}
