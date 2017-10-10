namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class MvvmLight
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetMetadataReferences();
            }

            [Test]
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set { this.Set(↓ref this.name, value); }
        }
    }
}";

                var fixedCode = @"
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
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyEmptyIf()
            {
                var testCode = @"
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
                if (this.Set(↓ref this.name, value))
                {
                }
            }
        }
    }
}";

                var fixedCode = @"
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
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(↓ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                }
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                    this.RaisePropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(↓ref this.name, value))
                    this.RaisePropertyChanged(nameof(this.Greeting1));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                    this.RaisePropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void RaisePropertyChangedAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
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

                ↓this.firstName = value;
                this.RaisePropertyChanged();
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
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.FullName));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
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
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.FullName));
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
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.FullName));
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            }
        }
    }
}