namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC010Benchmarks : AnalyzerBenchmarks
    {
        public INPC010Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC010SetAndReturnSameField())
        {
        }
    }
}