namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class PrismBindableBase
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.Prism;

            [Test]
            public static void NoCheckAddIfReturn()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged(nameof(this.Name));
            }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
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
                this.OnPropertyChanged(nameof(this.Name));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Check that value is different before notifying.", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Check that value is different before notifying.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void NoCheckToUseTrySet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged(nameof(this.Name));
            }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void NoCheckExpressionToUseTrySet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged(() => this.Name);
            }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var before = @"
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
                this.SetProperty(ref this.name, value);
                ↓this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyInternalClassInternalProperty()
            {
                var before = @"
namespace RoslynSandbox
{
    internal class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string name;

        internal string Greeting => $""Hello {this.Name}"";

        internal string Name
        {
            get { return this.name; }
            set
            {
                this.SetProperty(ref this.name, value);
                ↓this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    internal class ViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
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
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var before = @"
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
                }

                ↓this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsSecondCalculatedProperty()
            {
                var before = @"
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
                }

                ↓this.OnPropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var before = @"
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
                    this.OnPropertyChanged(nameof(this.Greeting1));
                ↓this.OnPropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, metadataReferences: MetadataReferences);
            }
        }
    }
}
