namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC007Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC007MissingInvoker());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
