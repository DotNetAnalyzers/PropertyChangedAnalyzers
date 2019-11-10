namespace PropertyChangedAnalyzers.Test.INPC012DoNotUseExpressionTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class MvvmCrossCore
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmCross;

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        public int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.RaisePropertyChanged(↓() => this.Greeting);
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        public int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionInternalClassInternalProperty()
            {
                var before = @"
namespace N
{
    internal class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        internal int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.RaisePropertyChanged(↓() => this.Greeting);
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    internal class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        internal int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }
        }
    }
}
