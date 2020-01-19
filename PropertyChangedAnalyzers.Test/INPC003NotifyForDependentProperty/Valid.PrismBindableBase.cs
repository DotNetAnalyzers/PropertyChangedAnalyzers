namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class PrismBindableBase
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.Prism;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
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
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
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
            public static void SetAffectsCalculatedPropertyNameOf()
            {
                var code = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
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
                    this.OnPropertyChanged(nameof(P1));
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
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
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
                    this.OnPropertyChanged(() => this.P1);
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
