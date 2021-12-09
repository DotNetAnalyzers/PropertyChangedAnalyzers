namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class PrismBindableBase
        {
            private static readonly Settings Settings = LibrarySettings.Prism;

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set { this.SetProperty(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyInternalClassInternalProperty()
            {
                var before = @"
namespace N
{
    internal class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        internal string P1 => $""Hello {this.P2}"";

        internal string P2
        {
            get { return this.p2; }
            set { this.SetProperty(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N
{
    internal class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        internal string P1 => $""Hello {this.P2}"";

        internal string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionBodyGetter()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1
        {
            get => $""Hello {this.P2}"";
        }

        public string P2
        {
            get { return this.p2; }
            set { this.SetProperty(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1
        {
            get => $""Hello {this.P2}"";
        }

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void SetExpressionBodiesAffectsCalculatedProperty()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get => this.p2;
            set => this.SetProperty(↓ref this.p2, value);
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get => this.p2;
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(↓ref this.p2, value))
                {
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void SetAffectsSecondCalculatedProperty()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(↓ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P11));
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P11));
                    this.OnPropertyChanged(nameof(this.P12));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(↓ref this.p2, value))
                    this.OnPropertyChanged(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P11));
                    this.OnPropertyChanged(nameof(this.P12));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void OnPropertyChangedAffectsCalculatedProperty()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
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
                this.OnPropertyChanged(nameof(this.FirstName));
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

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void IfNotSetReturnSetAffectsSecondCalculatedProperty()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(↓ref this.p2, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(ref this.p2, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.P11));
                this.OnPropertyChanged(nameof(this.P12));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void IfNotSetReturnSetAffectsSecondCalculatedPropertyNoBraces()
            {
                var before = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(↓ref this.p2, value))
                    return;
                
                this.OnPropertyChanged(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(ref this.p2, value))
                    return;
                
                this.OnPropertyChanged(nameof(this.P11));
                this.OnPropertyChanged(nameof(this.P12));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }
        }
    }
}
