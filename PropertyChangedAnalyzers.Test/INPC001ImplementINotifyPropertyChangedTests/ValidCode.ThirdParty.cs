namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public partial class ValidCode
    {
        internal class ThirdParty
        {
            [TearDown]
            public void TearDown()
            {
                RoslynAssert.ResetMetadataReferences();
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

                RoslynAssert.MetadataReferences.AddRange(MetadataReferences.Transitive(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly));
                RoslynAssert.Valid(Analyzer, testCode);
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

                RoslynAssert.MetadataReferences.AddRange(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly));
                RoslynAssert.Valid(Analyzer, testCode);
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

                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.Stylet);
                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void MvvmCross()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int Bar { get; set; }
    }
}";

                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.MvvmCross);
                RoslynAssert.Valid(Analyzer, testCode);
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
                RoslynAssert.AddTransitiveMetadataReferences(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase).Assembly);
                RoslynAssert.Valid(Analyzer, testCode);
            }
        }
    }
}
