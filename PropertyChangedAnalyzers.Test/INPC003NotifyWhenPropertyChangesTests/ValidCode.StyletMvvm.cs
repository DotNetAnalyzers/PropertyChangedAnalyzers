namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public partial class ValidCode
    {
        internal class StyletMvvm
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.AddRange(SpecialMetadataReferences.Stylet);
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetPropertyExpressionBodies()
            {
                var testCode = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyStringEmpty()
            {
                var testCode = @"
namespace RoslynSandbox
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyExpression()
            {
                var testCode = @"
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
                    this.NotifyOfPropertyChange(() => this.Greeting);
                }
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void WhenOverriddenSetAndNotify()
            {
                var fooBaseCode = @"
namespace RoslynSandbox
{
    public abstract class FooBase : Stylet.PropertyChangedBase
    {
        protected override bool SetAndNotify<T>(ref T oldValue, T newValue,[System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            return base.SetAndNotify(ref oldValue, newValue, propertyName);
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
            set { this.SetAndNotify(ref this.value, value); }
        }
    }
}";

                AnalyzerAssert.Valid(Analyzer, fooBaseCode, testCode);
            }
        }
    }
}
