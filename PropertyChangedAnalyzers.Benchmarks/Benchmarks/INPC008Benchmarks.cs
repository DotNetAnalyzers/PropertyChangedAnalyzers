namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC008Benchmarks : AnalyzerBenchmarks
    {
        public INPC008Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC008StructMustNotNotify())
        {
        }
    }
}