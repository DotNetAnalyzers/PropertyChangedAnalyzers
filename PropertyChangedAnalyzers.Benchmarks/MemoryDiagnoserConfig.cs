[assembly: BenchmarkDotNet.Attributes.Config(typeof(PropertyChangedAnalyzers.Benchmarks.MemoryDiagnoserConfig))]
namespace PropertyChangedAnalyzers.Benchmarks
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;

    public class MemoryDiagnoserConfig : ManualConfig
    {
        public MemoryDiagnoserConfig()
        {
            this.Add(new MemoryDiagnoser());
        }
    }
}