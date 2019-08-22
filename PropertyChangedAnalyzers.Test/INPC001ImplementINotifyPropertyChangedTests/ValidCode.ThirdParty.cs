namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class ValidCode
    {
        public static class ThirdParty
        {
            [Test]
            public static void MvvmLight()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        public int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.MvvmLight);
            }

            [Test]
            public static void CaliburnMicro()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.CaliburnMicro);
            }

            [Test]
            public static void Stylet()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.Stylet);
            }

            [Test]
            public static void MvvmCross()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.MvvmCross);
            }

            [Test]
            public static void SubclassBindableBase()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        public int Bar { get; set; }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.Prism);
            }
        }
    }
}
