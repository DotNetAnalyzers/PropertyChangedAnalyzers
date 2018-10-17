namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class ValidCode
    {
        internal class PrismBindableBase
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase).Assembly);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
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
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyEmptyIf()
            {
                var testCode = @"
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
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                    this.OnPropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                    this.OnPropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void OnPropertyChangedAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string firstName;
        private string lastName;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged(nameof(this.FirstName));
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged(nameof(this.LastName));
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void IfNotSetReturnCalculatedProperty()
            {
                var testCode = @"
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
                if (!SetProperty(ref this.name, value))
                {
                    return;
                }

                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }
        }
    }
}
