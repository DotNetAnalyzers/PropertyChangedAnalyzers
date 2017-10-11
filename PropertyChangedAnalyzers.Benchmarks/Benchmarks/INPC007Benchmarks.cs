namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC007Benchmarks : AnalyzerBenchmarks
    {
        public INPC007Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC007MissingInvoker())
        {
        }
    }
}