namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC001Benchmarks : AnalyzerBenchmarks
    {
        public INPC001Benchmarks()
            : base(new PropertyChangedAnalyzers.INPC001ImplementINotifyPropertyChanged())
        {
        }
    }
}