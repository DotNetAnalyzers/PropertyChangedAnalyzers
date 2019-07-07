namespace PropertyChangedAnalyzers.Test.INPC012DontUseExpressionTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
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
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var before = @"
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
                    this.RaisePropertyChanged(↓() => this.Greeting);
                }
            }
        }
    }
}";

                var after = @"
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
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionInternalClassInternalProperty()
            {
                var before = @"
namespace RoslynSandbox
{
    internal class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        internal int Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(↓() => this.Greeting);
                }
            }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    internal class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        internal int Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }
        }
    }
}
