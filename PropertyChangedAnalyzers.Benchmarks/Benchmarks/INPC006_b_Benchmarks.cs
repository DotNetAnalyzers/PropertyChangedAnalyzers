namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC006_b_Benchmarks : AnalyzerBenchmarks
    {
        public INPC006_b_Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC006UseObjectEqualsForReferenceTypes())
        {
        }
    }
}