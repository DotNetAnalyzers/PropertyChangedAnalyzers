namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        internal class ThirdParty
        {
            [TearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetMetadataReferences();
            }

            [Test]
            public void MvvmLight()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int value;

        public int Value
        {
            get => value;
            set => this.Set(ref this.value, value);
        }
    }
}";

                AnalyzerAssert.MetadataReferences.Add(MetadataReference.CreateFromFile(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly.Location));
                AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
            }
        }
    }
}