namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class ValidCode
    {
        public static class MvvmCrossCore
        {
            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.MvvmCross);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetAll();
            }

            [Test]
            public static void SetProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void SetPropertyExpressionBodies()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
                    this.RaisePropertyChanged(nameof(Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
                    this.RaisePropertyChanged(() => this.Greeting);
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void WhenOverriddenSetProperty()
            {
                var fooBaseCode = @"
namespace RoslynSandbox
{
    public abstract class FooBase : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        protected override bool SetProperty<T>(ref T oldValue, T newValue,[System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            return base.SetProperty(ref oldValue, newValue, propertyName);
        }
    }
}";

                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.SetProperty(ref this.value, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, fooBaseCode, testCode);
            }
        }
    }
}
