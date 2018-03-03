// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class IfStatementAnalyzerBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.IfStatementAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
