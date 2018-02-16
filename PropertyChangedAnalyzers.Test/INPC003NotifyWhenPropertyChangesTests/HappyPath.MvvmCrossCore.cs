namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class HappyPath
    {
        internal class MvvmCrossCore
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.AddRange(SpecialMetadataReferences.MvvmCrossReferences);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void SetProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value) }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetPropertyExpressionBodies()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get => return this.name;
            set => this.SetProperty(ref this.name, value)
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyExpression()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void WhenOverriddenSetProperty()
            {
                var fooBaseCode = @"
namespace RoslynSandbox
{
    public abstract class FooBase : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        public override bool Set<T>(ref T oldValue, T newValue,[System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
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

                AnalyzerAssert.Valid(Analyzer, fooBaseCode, testCode);
            }
        }
    }
}