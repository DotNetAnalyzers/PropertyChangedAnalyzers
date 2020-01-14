namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class MvvmLight
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmLight;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p;

        public string P
        {
            get { return this.p; }
            set { this.Set(ref this.p, value); }
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
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.Set(ref this.p, value);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyNameOf()
            {
                var code = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
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
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello{this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(() => this.P1);
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }
        }
    }
}
