namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class MvvmCrossCore
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmCross;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p;

        public string P
        {
            get { return this.p; }
            set { this.SetProperty(ref this.p, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetPropertyExpressionBodies()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.SetProperty(ref this.p, value);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(P1));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p2;

        public string P1 => $""Hello{this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(() => this.P1);
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WhenOverriddenSetProperty()
            {
                var viewModelBase = @"
namespace N
{
    public abstract class ViewModelBase : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        protected override bool SetProperty<T>(ref T oldValue, T newValue,[System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            return base.SetProperty(ref oldValue, newValue, propertyName);
        }
    }
}";

                var code = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.SetProperty(ref this.p, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, new[] { viewModelBase, code }, metadataReferences: MetadataReferences);
            }
        }
    }
}
