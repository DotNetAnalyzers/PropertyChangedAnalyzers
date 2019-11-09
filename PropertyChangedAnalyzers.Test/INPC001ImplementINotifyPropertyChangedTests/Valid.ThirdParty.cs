namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class ThirdParty
        {
            [Test]
            public static void MvvmLight()
            {
                var code = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
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
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        public int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.CaliburnMicro);
            }

            [Test]
            public static void Stylet()
            {
                var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        public int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.Stylet);
            }

            [Test]
            public static void MvvmCross()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.MvvmCross);
            }

            [Test]
            public static void SubclassBindableBase()
            {
                var code = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        public int P { get; set; }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: SpecialMetadataReferences.Prism);
            }
        }
    }
}
