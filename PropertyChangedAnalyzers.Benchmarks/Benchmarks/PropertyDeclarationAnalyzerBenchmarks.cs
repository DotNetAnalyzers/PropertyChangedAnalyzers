// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class PropertyDeclarationAnalyzerBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.PropertyDeclarationAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
