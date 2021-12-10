namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            [Test]
            public static void AssignedAffectsCalculatedPropertyOnPropertyChanged()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
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
                this.OnPropertyChanged();
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
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
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
                this.OnPropertyChanged();
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
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void IfNotSetReturnSetAffectsSecondCalculatedProperty()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.TrySet(↓ref this.p2, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.TrySet(ref this.p2, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.P11));
                this.OnPropertyChanged(nameof(this.P12));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void IfNotSetReturnSetAffectsSecondCalculatedPropertyNoBraces()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.TrySet(↓ref this.p2, value))
                    return;
                
                this.OnPropertyChanged(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!this.TrySet(ref this.p2, value))
                    return;
                
                this.OnPropertyChanged(nameof(this.P11));
                this.OnPropertyChanged(nameof(this.P12));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set { this.TrySet(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionBodyGetter()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1
        {
            get => $""Hello {this.P2}"";
        }

        public string P2
        {
            get { return this.p2; }
            set { this.TrySet(↓ref this.p2, value); }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
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
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void SetExpressionBodiesAffectsCalculatedProperty()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get => this.p2;
            set => this.TrySet(↓ref this.p2, value);
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get => this.p2;
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(↓ref this.p2, value))
                {
                }
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void SetAffectsSecondCalculatedProperty()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(↓ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P11));
                }
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P11));
                    this.OnPropertyChanged(nameof(this.P12));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(↓ref this.p2, value))
                    this.OnPropertyChanged(nameof(this.P11));
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P11 => $""Hello {this.P2}"";

        public string P12 => $""Hej {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P11));
                    this.OnPropertyChanged(nameof(this.P12));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }
        }
    }
}
