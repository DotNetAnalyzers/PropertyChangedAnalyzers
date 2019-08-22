namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class ValidCode
    {
        public static class PrismBindableBase
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.Prism;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetPropertyExpressionBodies()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyNameOf()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(Greeting));
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
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
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
                    this.OnPropertyChanged(() => this.Greeting);
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
