namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
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
                this.SetProperty(ref this.name, value);
                ↓this.RaisePropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
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
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyInternalClassInternalProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        internal string Greeting => $""Hello {this.Name}"";

        internal string Name
        {
            get { return this.name; }
            set
            {
                this.SetProperty(ref this.name, value);
                ↓this.RaisePropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    internal class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        internal string Greeting => $""Hello {this.Name}"";

        internal string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyEmptyIf()
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
                }

                ↓this.RaisePropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
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
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
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
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                }

                ↓this.RaisePropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
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
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                    this.RaisePropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
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
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                ↓this.RaisePropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
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
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                    this.RaisePropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }
        }
    }
}