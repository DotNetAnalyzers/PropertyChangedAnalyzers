// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class PropertyDeclarationAnalyzerBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.PropertyDeclarationAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
