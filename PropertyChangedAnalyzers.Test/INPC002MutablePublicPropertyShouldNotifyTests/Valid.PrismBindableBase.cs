namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public partial class Valid
    {
        public static class PrismBindableBase
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.Prism;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace N
{
    public class Foo : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.SetProperty(ref this.value, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetPropertyExpressionBodies()
            {
                var code = @"
namespace N
{
    public class Foo : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private int value;

        public int Value
        {
            get => value;
            set => this.SetProperty(ref this.value, value);
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [TestCase("null")]
            [TestCase("string.Empty")]
            [TestCase(@"""Bar""")]
            [TestCase(@"nameof(Bar)")]
            [TestCase(@"nameof(this.Bar)")]
            public static void OnPropertyChanged(string propertyName)
            {
                var code = @"
namespace N
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private int bar;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }
    }
}".AssertReplace(@"nameof(this.Bar)", propertyName);

                RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, metadataReferences: MetadataReferences);
            }
        }
    }
}