namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC009Benchmarks : AnalyzerBenchmarks
    {
        public INPC009Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC009DontRaiseChangeForMissingProperty())
        {
        }
    }
}