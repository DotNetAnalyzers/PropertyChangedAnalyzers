// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC001ImplementINotifyPropertyChangedBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC001ImplementINotifyPropertyChanged());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC003NotifyWhenPropertyChangesBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC003NotifyWhenPropertyChanges());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC004UseCallerMemberNameBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC004UseCallerMemberName());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC007MissingInvokerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC007MissingInvoker());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC008StructMustNotNotifyBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC008StructMustNotNotify());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC011DontShadowBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC011DontShadow());

        private static readonly Gu.Roslyn.Asserts.Benchmark ArgumentAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.ArgumentAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark AssignmentAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.AssignmentAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark IfStatementAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.IfStatementAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark InvocationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.InvocationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark PropertyDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.PropertyDeclarationAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC001ImplementINotifyPropertyChanged()
        {
            INPC001ImplementINotifyPropertyChangedBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC003NotifyWhenPropertyChanges()
        {
            INPC003NotifyWhenPropertyChangesBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC004UseCallerMemberName()
        {
            INPC004UseCallerMemberNameBenchmark.Run();
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
        public void IfStatementAnalyzer()
        {
            IfStatementAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void InvocationAnalyzer()
        {
            InvocationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void PropertyDeclarationAnalyzer()
        {
            PropertyDeclarationAnalyzerBenchmark.Run();
        }
    }
}
