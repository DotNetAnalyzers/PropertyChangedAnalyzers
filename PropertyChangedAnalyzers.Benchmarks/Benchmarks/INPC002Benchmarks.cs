namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC002Benchmarks : AnalyzerBenchmarks
    {
        public INPC002Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC002MutablePublicPropertyShouldNotify())
        {
        }
    }
}