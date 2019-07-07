namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public partial class ValidCode
    {
        public static class PrismBindableBase
        {
            [OneTimeSetUp]
            public static void OneTimeSetPropertyUp()
            {
                RoslynAssert.AddTransitiveMetadataReferences(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase).Assembly);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetAll();
            }

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace RoslynSandbox
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

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void SetPropertyExpressionBodies()
            {
                var code = @"
namespace RoslynSandbox
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

                RoslynAssert.Valid(Analyzer, code);
            }

            [TestCase("null")]
            [TestCase("string.Empty")]
            [TestCase(@"""Bar""")]
            [TestCase(@"nameof(Bar)")]
            [TestCase(@"nameof(this.Bar)")]
            public static void OnPropertyChanged(string propertyName)
            {
                var code = @"
namespace RoslynSandbox
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

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }
        }
    }
}
