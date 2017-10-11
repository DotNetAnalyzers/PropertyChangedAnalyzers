namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC011Benchmarks : AnalyzerBenchmarks
    {
        public INPC011Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC011DontShadow())
        {
        }
    }
}