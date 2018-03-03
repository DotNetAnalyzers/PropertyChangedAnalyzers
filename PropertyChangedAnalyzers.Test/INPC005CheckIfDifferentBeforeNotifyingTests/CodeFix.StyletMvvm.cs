namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class CodeFix
    {
        internal class StyletMvvm
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.Add(SpecialMetadataReferences.Stylet);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void NoCheckAddIfReturn()
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
            set
            {
                this.name = value;
                ↓this.NotifyOfPropertyChange();
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.NotifyOfPropertyChange();
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, fixTitle: "Check that value is different before notifying.");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, fixTitle: "Check that value is different before notifying.");
            }

            [Test]
            public void NoCheckToUseSetAndRaise()
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
            set
            {
                this.name = value;
                ↓this.NotifyOfPropertyChange();
            }
        }
    }
}";

                var fixedCode = @"
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
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, fixTitle: "Use PropertyChangedBase.SetAndNotify");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, fixTitle: "Use PropertyChangedBase.SetAndNotify");
            }

            [Test]
            public void NoCheckExpressionToUseSetAndRaise()
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
            set
            {
                this.name = value;
                ↓this.NotifyOfPropertyChange(() => this.Name);
            }
        }
    }
}";

                var fixedCode = @"
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
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, fixTitle: "Use PropertyChangedBase.SetAndNotify");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, fixTitle: "Use PropertyChangedBase.SetAndNotify");
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
                this.SetAndNotify(ref this.name, value);
                ↓this.NotifyOfPropertyChange(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
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
                    this.NotifyOfPropertyChange(nameof(this.Greeting));
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
    internal class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        internal string Greeting => $""Hello {this.Name}"";

        internal string Name
        {
            get { return this.name; }
            set
            {
                this.SetAndNotify(ref this.name, value);
                ↓this.NotifyOfPropertyChange(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    internal class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        internal string Greeting => $""Hello {this.Name}"";

        internal string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting));
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
                }

                ↓this.NotifyOfPropertyChange(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
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
                    this.NotifyOfPropertyChange(nameof(this.Greeting));
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
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting1));
                }

                ↓this.NotifyOfPropertyChange(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting1));
                    this.NotifyOfPropertyChange(nameof(this.Greeting2));
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
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                    this.NotifyOfPropertyChange(nameof(this.Greeting1));
                ↓this.NotifyOfPropertyChange(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetAndNotify(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting1));
                    this.NotifyOfPropertyChange(nameof(this.Greeting2));
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