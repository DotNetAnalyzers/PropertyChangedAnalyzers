namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC013Benchmarks : AnalyzerBenchmarks
    {
        public INPC013Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC013UseNameof())
        {
        }
    }
}