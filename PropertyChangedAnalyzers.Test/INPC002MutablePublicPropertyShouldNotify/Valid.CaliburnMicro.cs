namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class CaliburnMicro
        {
            private static readonly Settings MetadataReferences = LibrarySettings.CaliburnMicro;

            [Test]
            public static void Set()
            {
                var code = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get { return p; }
            set { this.Set(ref this.p, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: MetadataReferences);
            }

            [Test]
            public static void SetExpressionBodies()
            {
                var code = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get => p;
            set => this.Set(ref this.p, value);
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code, settings: MetadataReferences);
            }

            [TestCase("null")]
            [TestCase("string.Empty")]
            [TestCase(@"""P""")]
            [TestCase(@"nameof(P)")]
            [TestCase(@"nameof(this.P)")]
            public static void RaisePropertyChanged(string propertyName)
            {
                var code = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.NotifyOfPropertyChange(nameof(P));
            }
        }
    }
}".AssertReplace(@"nameof(P)", propertyName);

                RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: MetadataReferences);
            }
        }
    }
}
