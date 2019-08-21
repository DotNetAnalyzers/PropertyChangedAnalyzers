namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class MvvmLight
        {
            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.AddTransitiveMetadataReferences(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly);
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
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.Set(ref this.name, value); }
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
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.Set(ref this.name, value);
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
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(Greeting));
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
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        public int Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(() => this.Greeting);
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
