namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class StyletMvvm
        {
            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.Stylet);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetAll();
            }

            [Test]
            public static void NoCheckAddIfReturn()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Check that value is different before notifying.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Check that value is different before notifying.");
            }

            [Test]
            public static void NoCheckToUseSetAndRaise()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify<T>(ref T field, T value, string propertyName)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify<T>(ref T field, T value, string propertyName)");
            }

            [Test]
            public static void NoCheckExpressionToUseSetAndRaise()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify<T>(ref T field, T value, string propertyName)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify<T>(ref T field, T value, string propertyName)");
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyInternalClassInternalProperty()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void SetAffectsSecondCalculatedProperty()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var before = @"
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

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }
        }
    }
}
