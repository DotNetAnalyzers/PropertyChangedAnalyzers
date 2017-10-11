namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC012Benchmarks : AnalyzerBenchmarks
    {
        public INPC012Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC012DontUseExpression())
        {
        }
    }
}