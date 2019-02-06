// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class INPC001ImplementINotifyPropertyChangedBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.INPC001ImplementINotifyPropertyChanged());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
