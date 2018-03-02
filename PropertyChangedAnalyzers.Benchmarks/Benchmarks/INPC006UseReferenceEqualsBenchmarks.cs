// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC006UseReferenceEqualsBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC006UseReferenceEquals());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
