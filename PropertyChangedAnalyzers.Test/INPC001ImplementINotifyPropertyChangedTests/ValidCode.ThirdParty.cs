namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class ValidCode
    {
        public static class ThirdParty
        {
            [TearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetMetadataReferences();
            }

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

                RoslynAssert.MetadataReferences.AddRange(MetadataReferences.Transitive(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly));
                RoslynAssert.Valid(Analyzer, code);
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

                RoslynAssert.MetadataReferences.AddRange(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly));
                RoslynAssert.Valid(Analyzer, code);
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

                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.Stylet);
                RoslynAssert.Valid(Analyzer, code);
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

                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.MvvmCross);
                RoslynAssert.Valid(Analyzer, code);
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
                RoslynAssert.AddTransitiveMetadataReferences(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase).Assembly);
                RoslynAssert.Valid(Analyzer, code);
            }
        }
    }
}
