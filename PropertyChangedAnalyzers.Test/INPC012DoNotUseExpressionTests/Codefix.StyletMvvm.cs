namespace PropertyChangedAnalyzers.Test.INPC012DoNotUseExpressionTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class StyletMvvm
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.Stylet;

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        public int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(↓() => this.Greeting);
                }
            }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        public int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting));
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
namespace RoslynSandbox
{
    internal class ViewModel : Stylet.PropertyChangedBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        internal int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(↓() => this.Greeting);
                }
            }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    internal class ViewModel : Stylet.PropertyChangedBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        internal int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting));
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
