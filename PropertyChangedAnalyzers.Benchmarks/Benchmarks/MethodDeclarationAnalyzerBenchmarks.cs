// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class MethodDeclarationAnalyzerBenchmarks
{
    private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.MethodDeclarationAnalyzer());

    [BenchmarkDotNet.Attributes.Benchmark]
    public void RunOnValidCodeProject()
    {
        Benchmark.Run();
    }
}
