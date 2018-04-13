namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class HappyPath
    {
        internal class ReactiveUI
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.Add(SpecialMetadataReferences.ReactiveUIReferences);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            [Category("ThirdParty")]
            [Category("ReactiveUI")]
            public void BasicReactiveObjectExpresssionBody()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using ReativeUI;
    public class ViewModel : ReactiveObject
    {
        private string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            [Category("ThirdParty")]
            [Category("ReactiveUI")]
            public void BasicReactiveObject()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using ReactiveUI;
    public class ViewModel : ReactiveObject
    {
        private string name;
        public string Name
        {
            get => name;
            set { this.RaiseAndSetIfChanged(ref name, value); }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            [Category("ThirdParty")]
            [Category("ReactiveUI")]
            public void ShouldFail()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using ReactiveUI;
    public class ViewModel : ReactiveObject
    {
        private string name;
        public string Name
        {
            get => name;
            set { name=value; }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }
        }
    }
}
