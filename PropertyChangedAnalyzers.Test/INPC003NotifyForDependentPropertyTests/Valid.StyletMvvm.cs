namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class StyletMvvm
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.Stylet;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetAndNotify(ref this.name, value); }
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
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetAndNotify(ref this.name, value);
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
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyStringEmpty()
            {
                var code = @"
namespace N
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get => this.name;
            set => this.SetAndNotify(ref this.name, value, string.Empty);
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
                    this.NotifyOfPropertyChange(() => this.Greeting);
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WhenOverriddenSetAndNotify()
            {
                var viewModelBase = @"
namespace N
{
    public abstract class ViewModelBase : Stylet.PropertyChangedBase
    {
        protected override bool SetAndNotify<T>(ref T oldValue, T newValue,[System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            return base.SetAndNotify(ref oldValue, newValue, propertyName);
        }
    }
}";

                var code = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.SetAndNotify(ref this.value, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, new[] { viewModelBase, code }, metadataReferences: MetadataReferences);
            }
        }
    }
}
