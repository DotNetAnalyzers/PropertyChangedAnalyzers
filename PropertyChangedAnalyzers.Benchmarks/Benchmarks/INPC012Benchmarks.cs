namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC012Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC012DontUseExpression());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
