namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC005Benchmarks : AnalyzerBenchmarks
    {
        public INPC005Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC005CheckIfDifferentBeforeNotifying())
        {
        }
    }
}