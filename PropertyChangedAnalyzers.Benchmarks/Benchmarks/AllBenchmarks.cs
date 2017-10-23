// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier
namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC001 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC001ImplementINotifyPropertyChanged());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC002 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC002MutablePublicPropertyShouldNotify());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC003 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC003NotifyWhenPropertyChanges());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC004 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC004UseCallerMemberName());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC005 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC005CheckIfDifferentBeforeNotifying());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC006b = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC006UseObjectEqualsForReferenceTypes());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC006a = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC006UseReferenceEquals());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC007 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC007MissingInvoker());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC008 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC008StructMustNotNotify());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC009 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC009DontRaiseChangeForMissingProperty());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC010 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC010SetAndReturnSameField());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC011 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC011DontShadow());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC012 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC012DontUseExpression());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC013 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC013UseNameof());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC001ImplementINotifyPropertyChanged()
        {
            INPC001.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC002MutablePublicPropertyShouldNotify()
        {
            INPC002.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC003NotifyWhenPropertyChanges()
        {
            INPC003.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC004UseCallerMemberName()
        {
            INPC004.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC005CheckIfDifferentBeforeNotifying()
        {
            INPC005.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC006UseObjectEqualsForReferenceTypes()
        {
            INPC006b.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC006UseReferenceEquals()
        {
            INPC006a.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC007MissingInvoker()
        {
            INPC007.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC008StructMustNotNotify()
        {
            INPC008.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC009DontRaiseChangeForMissingProperty()
        {
            INPC009.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC010SetAndReturnSameField()
        {
            INPC010.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC011DontShadow()
        {
            INPC011.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC012DontUseExpression()
        {
            INPC012.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC013UseNameof()
        {
            INPC013.Run();
        }
    }
}
