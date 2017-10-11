namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC004Benchmarks : AnalyzerBenchmarks
    {
        public INPC004Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC004UseCallerMemberName())
        {
        }
    }
}