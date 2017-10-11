namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using Microsoft.CodeAnalysis.Diagnostics;

    public abstract class AnalyzerBenchmarks
    {
        private readonly BenchmarkWalker walker;

        protected AnalyzerBenchmarks(DiagnosticAnalyzer analyzer)
        {
            this.walker = new BenchmarkWalker(Code.AnalyzersProject, analyzer);
        }

        [Benchmark]
        public void RunOnPropertyChangedAnalyzersAnalyzers()
        {
            this.walker.Run();
        }
    }
}
