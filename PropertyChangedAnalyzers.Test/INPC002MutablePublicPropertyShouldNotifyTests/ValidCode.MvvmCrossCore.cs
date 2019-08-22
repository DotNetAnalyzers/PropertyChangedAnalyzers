namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class ValidCode
    {
        public static class MvvmCrossCore
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmCross;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
            public static void SetExpressionBodies()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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

            [TestCase("(string)null")]
            [TestCase("string.Empty")]
            [TestCase(@"""Bar""")]
            [TestCase(@"nameof(Bar)")]
            [TestCase(@"nameof(this.Bar)")]
            public static void RaisePropertyChanged(string propertyName)
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.RaisePropertyChanged(nameof(Bar));
            }
        }
    }
}".AssertReplace(@"nameof(Bar)", propertyName);

                RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, metadataReferences: MetadataReferences);
            }
        }
    }
}
