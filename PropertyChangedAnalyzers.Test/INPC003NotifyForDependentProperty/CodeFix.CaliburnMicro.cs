namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class CaliburnMicro
        {
            private static readonly Settings Settings = LibrarySettings.CaliburnMicro;

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set { this.Set(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
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
    internal class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        internal string P1 => $""Hello {this.P2}"";

        internal string P2
        {
            get { return this.p2; }
            set { this.Set(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N
{
    internal class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        internal string P1 => $""Hello {this.P2}"";

        internal string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
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
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1
        {
            get => $""Hello {this.P2}"";
        }

        public string P2
        {
            get { return this.p2; }
            set { this.Set(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
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
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
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
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get => this.p2;
            set => this.Set(↓ref this.p2, value);
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get => this.p2;
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
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
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(↓ref this.p2, value))
                {
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
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
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(↓ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P11));
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P11));
                    this.NotifyOfPropertyChange(nameof(this.P12));
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
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(↓ref this.p2, value))
                    this.NotifyOfPropertyChange(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P11));
                    this.NotifyOfPropertyChange(nameof(this.P12));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void NotifyOfPropertyChangeAffectsCalculatedProperty()
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
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
                this.NotifyOfPropertyChange();
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
                this.NotifyOfPropertyChange();
                this.NotifyOfPropertyChange(nameof(this.FullName));
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
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
                this.NotifyOfPropertyChange();
                this.NotifyOfPropertyChange(nameof(this.FullName));
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
                this.NotifyOfPropertyChange();
                this.NotifyOfPropertyChange(nameof(this.FullName));
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
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.Set(↓ref this.p2, value))
                {
                    return;
                }
                
                this.NotifyOfPropertyChange(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.Set(ref this.p2, value))
                {
                    return;
                }
                
                this.NotifyOfPropertyChange(nameof(this.P11));
                this.NotifyOfPropertyChange(nameof(this.P12));
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
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (!this.Set(↓ref this.p3, value))
                    return;
                
                this.NotifyOfPropertyChange(nameof(this.P1));
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (!this.Set(ref this.p3, value))
                    return;
                
                this.NotifyOfPropertyChange(nameof(this.P1));
                this.NotifyOfPropertyChange(nameof(this.P2));
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
