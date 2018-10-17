namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class ValidCode
    {
        internal class MvvmCrossCore
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.AddRange(SpecialMetadataReferences.MvvmCross);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void SetProperty()
            {
                var testCode = @"
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

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetExpressionBodies()
            {
                var testCode = @"
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

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [TestCase("(string)null")]
            [TestCase("string.Empty")]
            [TestCase(@"""Bar""")]
            [TestCase(@"nameof(Bar)")]
            [TestCase(@"nameof(this.Bar)")]
            public void RaisePropertyChanged(string propertyName)
            {
                var testCode = @"
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
}";

                testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
                AnalyzerAssert.Valid(Analyzer, testCode);
            }
        }
    }
}
