namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifying;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public static partial class CodeFix
{
    public static class MvvmLight
    {
        private static readonly Settings Settings = LibrarySettings.MvvmLight;

        [Test]
        public static void NoCheckAddIfReturn()
        {
            var before = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.RaisePropertyChanged();
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.RaisePropertyChanged();
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Check that value is different before notifying.", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Check that value is different before notifying.", settings: Settings);
        }

        [Test]
        public static void NoCheckToUseTrySet()
        {
            var before = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.RaisePropertyChanged();
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set => this.Set(ref this.p, value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref field, newValue)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref field, newValue)", settings: Settings);
        }

        [Test]
        public static void NoCheckExpressionToUseTrySet()
        {
            var before = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.RaisePropertyChanged(() => this.P);
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set => this.Set(ref this.p, value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref field, newValue)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref field, newValue)", settings: Settings);
        }

        [Test]
        public static void SetAffectsCalculatedProperty()
        {
            var before = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p2;

        public string P1 => $""Hello {this.p2}"";

        public int P2
        {
            get { return this.p2; }
            set
            {
                this.Set(ref this.p2, value)
                ↓this.RaisePropertyChanged(nameof(this.P1));
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p2;

        public string P1 => $""Hello {this.p2}"";

        public int P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
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
    internal class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p2;

        internal string P1 => $""Hello {this.p2}"";

        internal int P2
        {
            get { return this.p2; }
            set
            {
                this.Set(ref this.p2, value)
                ↓this.RaisePropertyChanged(nameof(this.P1));
            }
        }
    }
}";

            var after = @"
namespace N
{
    internal class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p2;

        internal string P1 => $""Hello {this.p2}"";

        internal int P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
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
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p2;

        public string P1 => $""Hello {this.p2}"";

        public int P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                }

                ↓this.RaisePropertyChanged(nameof(this.P1));
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p2;

        public string P1 => $""Hello {this.p2}"";

        public int P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
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
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public int P3
        {
            get { return this.p3; }
            set
            {
                if (this.Set(ref this.p3, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }

                ↓this.RaisePropertyChanged(nameof(this.P2));
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public int P3
        {
            get { return this.p3; }
            set
            {
                if (this.Set(ref this.p3, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                    this.RaisePropertyChanged(nameof(this.P2));
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
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public int P3
        {
            get { return this.p3; }
            set
            {
                if (this.Set(ref this.p3, value))
                    this.RaisePropertyChanged(nameof(this.P1));
                ↓this.RaisePropertyChanged(nameof(this.P2));
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public int P3
        {
            get { return this.p3; }
            set
            {
                if (this.Set(ref this.p3, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                    this.RaisePropertyChanged(nameof(this.P2));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
        }
    }
}
