namespace PropertyChangedAnalyzers.Benchmarks.Benchmarks
{
    using BenchmarkDotNet.Attributes;

    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC001 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC001ImplementINotifyPropertyChanged());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC002 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC002MutablePublicPropertyShouldNotify());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC003 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC003NotifyWhenPropertyChanges());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC004 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC004UseCallerMemberName());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC005 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC005CheckIfDifferentBeforeNotifying());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC006A = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC006UseObjectEqualsForReferenceTypes());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC006B = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC006UseReferenceEquals());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC007 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC007MissingInvoker());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC008 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC008StructMustNotNotify());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC009 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC009DontRaiseChangeForMissingProperty());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC010 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC010SetAndReturnSameField());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC011 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC011DontShadow());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC012 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC012DontUseExpression());
        private static readonly Gu.Roslyn.Asserts.Benchmark INPC013 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC013UseNameof());

        private static readonly Gu.Roslyn.Asserts.Benchmark INPC014 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new PropertyChangedAnalyzers.INPC014PreferSettingBackingFieldInCtor());
        [Benchmark]

        public void INPC001ImplementINotifyPropertyChanged()
        {
            INPC001.Run();
        }

        [Benchmark]
        public void INPC002MutablePublicPropertyShouldNotify()
        {
            INPC002.Run();
        }

        [Benchmark]
        public void INPC003NotifyWhenPropertyChanges()
        {
            INPC003.Run();
        }

        [Benchmark]
        public void INPC004UseCallerMemberName()
        {
            INPC004.Run();
        }

        [Benchmark]
        public void INPC005CheckIfDifferentBeforeNotifying()
        {
            INPC005.Run();
        }

        [Benchmark]
        public void INPC006UseObjectEqualsForReferenceTypes()
        {
            INPC006A.Run();
        }

        [Benchmark]
        public void INPC006UseReferenceEquals()
        {
            INPC006B.Run();
        }

        [Benchmark]
        public void INPC007MissingInvoker()
        {
            INPC007.Run();
        }

        [Benchmark]
        public void INPC008StructMustNotNotify()
        {
            INPC008.Run();
        }

        [Benchmark]
        public void INPC009DontRaiseChangeForMissingProperty()
        {
            INPC009.Run();
        }

        [Benchmark]
        public void INPC010SetAndReturnSameField()
        {
            INPC010.Run();
        }

        [Benchmark]
        public void INPC011DontShadow()
        {
            INPC011.Run();
        }

        [Benchmark]
        public void INPC012DontUseExpression()
        {
            INPC012.Run();
        }

        [Benchmark]
        public void INPC013UseNameof()
        {
            INPC013.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void INPC014PreferSettingBackingFieldInCtor()
        {
            INPC014.Run();
        }
    }
}