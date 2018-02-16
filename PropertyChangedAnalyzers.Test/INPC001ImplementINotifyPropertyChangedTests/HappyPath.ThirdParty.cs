namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

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
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.Add(MetadataReference.CreateFromFile(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly.Location));
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void CaliburnMicro()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.Add(MetadataReference.CreateFromFile(typeof(Caliburn.Micro.PropertyChangedBase).Assembly.Location));
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void Stylet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.Add(SpecialMetadataReferences.Stylet);
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void MvvmCross()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.MetadataReferences.Add(SpecialMetadataReferences.MvvmCross);
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SubclassBindableBase()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        public int Bar { get; set; }
    }
}";
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase).Assembly);
                AnalyzerAssert.Valid(Analyzer, testCode);
            }
        }
    }
}