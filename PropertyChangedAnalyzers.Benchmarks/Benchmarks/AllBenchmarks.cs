// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    [BenchmarkDotNet.Attributes.MemoryDiagnoser]
    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark ArgumentAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.ArgumentAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark AssignmentAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.AssignmentAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark ClassDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.ClassDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark EqualityAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.EqualityAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark EventAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.EventAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark InvocationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.InvocationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark MethodDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.MethodDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark MutationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.MutationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark PropertyDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.PropertyDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark SetAccessorAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.SetAccessorAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark StructAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.StructAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ArgumentAnalyzer()
        {
            ArgumentAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void AssignmentAnalyzer()
        {
            AssignmentAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ClassDeclarationAnalyzer()
        {
            ClassDeclarationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void EqualityAnalyzer()
        {
            EqualityAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void EventAnalyzer()
        {
            EventAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void InvocationAnalyzer()
        {
            InvocationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void MethodDeclarationAnalyzer()
        {
            MethodDeclarationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void MutationAnalyzer()
        {
            MutationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void PropertyDeclarationAnalyzer()
        {
            PropertyDeclarationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void SetAccessorAnalyzer()
        {
            SetAccessorAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void StructAnalyzer()
        {
            StructAnalyzerBenchmark.Run();
        }
    }
}
