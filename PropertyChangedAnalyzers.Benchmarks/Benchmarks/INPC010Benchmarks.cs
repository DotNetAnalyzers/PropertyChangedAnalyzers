namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC010Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC010SetAndReturnSameField());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
