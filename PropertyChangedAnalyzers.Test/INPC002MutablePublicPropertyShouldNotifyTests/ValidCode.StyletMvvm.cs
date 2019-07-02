namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public partial class ValidCode
    {
        public static class StyletMvvm
        {
            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.Stylet);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetAll();
            }

            [Test]
            public static void Set()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.SetAndNotify(ref this.value, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, testCode);
            }

            [Test]
            public static void SetExpressionBodies()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get => value;
            set => this.SetAndNotify(ref this.value, value);
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [TestCase("null")]
            [TestCase("string.Empty")]
            [TestCase(@"""Bar""")]
            [TestCase(@"nameof(Bar)")]
            [TestCase(@"nameof(this.Bar)")]
            public static void RaisePropertyChanged(string propertyName)
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private int bar;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.NotifyOfPropertyChange(nameof(Bar));
            }
        }
    }
}".AssertReplace(@"nameof(Bar)", propertyName);

                RoslynAssert.Valid(Analyzer, Descriptor, testCode);
            }
        }
    }
}
