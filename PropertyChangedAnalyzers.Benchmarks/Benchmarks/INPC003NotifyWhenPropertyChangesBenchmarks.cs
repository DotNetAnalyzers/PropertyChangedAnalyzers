// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC003NotifyWhenPropertyChangesBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.INPC003NotifyWhenPropertyChanges());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
