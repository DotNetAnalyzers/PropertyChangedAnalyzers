namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC006_a_Benchmarks : AnalyzerBenchmarks
    {
        public INPC006_a_Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC006UseReferenceEquals())
        {
        }
    }
}