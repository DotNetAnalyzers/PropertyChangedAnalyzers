namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public static partial class CodeFix
{
    public static class MvvmCrossCore
    {
        private static readonly Settings MetadataReferences = LibrarySettings.MvvmCross;

        [Test]
        public static void SetAffectsCalculatedProperty()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set { this.SetProperty(↓ref this.p2, value); }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyInternalClassInternalProperty()
        {
            var before = @"
namespace N
{
    internal class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        internal string P1 => $""Hello {this.P2}"";

        internal string? P2
        {
            get { return this.p2; }
            set { this.SetProperty(↓ref this.p2, value); }
        }
    }
}";

            var after = @"
namespace N
{
    internal class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        internal string P1 => $""Hello {this.P2}"";

        internal string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyExpressionBodyGetter()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1
        {
            get => $""Hello {this.P2}"";
        }

        public string? P2
        {
            get { return this.p2; }
            set { this.SetProperty(↓ref this.p2, value); }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1
        {
            get => $""Hello {this.P2}"";
        }

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void SetExpressionBodiesAffectsCalculatedProperty()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get => this.p2;
            set => this.SetProperty(↓ref this.p2, value);
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get => this.p2;
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyEmptyIf()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
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
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void SetAffectsSecondCalculatedProperty()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(↓ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P11));
                }
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P11));
                    this.RaisePropertyChanged(nameof(this.P12));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void SetAffectsSecondCalculatedPropertyMissingBraces()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(↓ref this.p2, value))
                    this.RaisePropertyChanged(nameof(this.P11));
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P11));
                    this.RaisePropertyChanged(nameof(this.P12));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void RaisePropertyChangedAffectsCalculatedProperty()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
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

        public string? LastName
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

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
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

        public string? LastName
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void IfNotSetReturnSetAffectsSecondCalculatedProperty()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(↓ref this.p2, value))
                {
                    return;
                }
                
                this.RaisePropertyChanged(nameof(this.P11));
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(ref this.p2, value))
                {
                    return;
                }
                
                this.RaisePropertyChanged(nameof(this.P11));
                this.RaisePropertyChanged(nameof(this.P12));
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }

        [Test]
        public static void IfNotSetReturnSetAffectsSecondCalculatedPropertyNoBraces()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(↓ref this.p2, value))
                    return;
                
                this.RaisePropertyChanged(nameof(this.P11));
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string? p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (!this.SetProperty(ref this.p2, value))
                    return;
                
                this.RaisePropertyChanged(nameof(this.P11));
                this.RaisePropertyChanged(nameof(this.P12));
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: MetadataReferences);
        }
    }
}
