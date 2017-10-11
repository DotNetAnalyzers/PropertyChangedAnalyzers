namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC003Benchmarks : AnalyzerBenchmarks
    {
        public INPC003Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC003NotifyWhenPropertyChanges())
        {
        }
    }
}