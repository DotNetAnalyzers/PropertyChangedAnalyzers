namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChanged
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class MvvmLight
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmLight;

            [Test]
            public static void SubclassViewModelBaseAddUsing()
            {
                var before = @"
namespace N
{
    public class ↓C
    {
        public int P { get; set; }
    }
}";

                var after = @"
namespace N
{
    using GalaSoft.MvvmLight;

    public class C : ViewModelBase
    {
        public int P { get; set; }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Subclass GalaSoft.MvvmLight.ViewModelBase and add using.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SubclassViewModelBaseFullyQualified()
            {
                var before = @"
namespace N
{
    public class ↓C
    {
        public int P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        public int P { get; set; }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Subclass GalaSoft.MvvmLight.ViewModelBase fully qualified.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void ImplementINotifyPropertyChangedAddUsings()
            {
                var before = @"
namespace N
{
    public class ↓C
    {
        public int P { get; set; }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void ImplementINotifyPropertyChangedFullyQualified()
            {
                var before = @"
namespace N
{
    public class ↓C
    {
        public int P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int P { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.", metadataReferences: MetadataReferences);
            }
        }
    }
}
