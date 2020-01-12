namespace PropertyChangedAnalyzers
{
    internal struct AnalysisResult<T>
    {
        internal readonly AnalysisResult Result;

        internal readonly T Value;

        internal AnalysisResult(AnalysisResult result, T value)
        {
            this.Result = result;
            this.Value = value;
        }
    }
}
