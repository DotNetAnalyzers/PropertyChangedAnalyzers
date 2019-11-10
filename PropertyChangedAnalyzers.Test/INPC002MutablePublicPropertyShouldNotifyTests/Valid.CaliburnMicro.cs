namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class CaliburnMicro
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.CaliburnMicro;

            [Test]
            public static void Set()
            {
                var code = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.Set(ref this.value, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetExpressionBodies()
            {
                var code = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get => value;
            set => this.Set(ref this.value, value);
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
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

                RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, metadataReferences: MetadataReferences);
            }
        }
    }
}
