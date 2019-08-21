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

        private static readonly Gu.Roslyn.Asserts.Benchmark InvocationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.InvocationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark MethodDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.MethodDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark PropertyDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.PropertyDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC003NotifyWhenPropertyChangesBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.INPC003NotifyWhenPropertyChanges());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC007MissingInvokerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.INPC007MissingInvoker());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC008StructMustNotNotifyBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.INPC008StructMustNotNotify());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC011DontShadowBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new PropertyChangedAnalyzers.INPC011DontShadow());

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
        public void PropertyDeclarationAnalyzer()
        {
            PropertyDeclarationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC003NotifyWhenPropertyChanges()
        {
            INPC003NotifyWhenPropertyChangesBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC007MissingInvoker()
        {
            INPC007MissingInvokerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC008StructMustNotNotify()
        {
            INPC008StructMustNotNotifyBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC011DontShadow()
        {
            INPC011DontShadowBenchmark.Run();
        }
    }
}
