namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class PrismBindableBase
        {
            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.AddTransitiveMetadataReferences(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase).Assembly);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetAll();
            }

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
                RoslynAssert.Valid(Analyzer, code);
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
                RoslynAssert.Valid(Analyzer, code);
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
                RoslynAssert.Valid(Analyzer, code);
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
                RoslynAssert.Valid(Analyzer, code);
            }
        }
    }
}
