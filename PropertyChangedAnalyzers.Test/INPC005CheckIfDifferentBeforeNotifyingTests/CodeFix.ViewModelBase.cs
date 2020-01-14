namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            [Test]
            public static void NoCheckAddIfReturn()
            {
                var before = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.OnPropertyChanged();
            }
        }
    }
}";

                var after = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Check that value is different before notifying.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Check that value is different before notifying.");
            }

            [Test]
            public static void NoCheckToUseTrySet()
            {
                var before = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.OnPropertyChanged();
            }
        }
    }
}";

                var after = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void NoCheckToUseTrySetUnderscoreNames()
            {
                var before = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string _p;

        public string P
        {
            get => _p;
            set
            {
                _p = value;
                ↓OnPropertyChanged();
            }
        }
    }
}";

                var after = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string _p;

        public string P
        {
            get => _p;
            set => TrySet(ref _p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void NoCheckExpressionToUseTrySet()
            {
                var before = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.OnPropertyChanged(() => this.P);
            }
        }
    }
}";

                var after = @"
namespace N.Client 
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void NoIfForTrySet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                this.TrySet(ref this.p2, value)
                ↓this.OnPropertyChanged(nameof(this.P1));
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

        public string P1 => $""Hello {this.p2}"";

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
            public static void OutsideIfTrySet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                }

                ↓this.OnPropertyChanged(nameof(this.P1));
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

        public string P1 => $""Hello {this.p2}"";

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
            public static void SetAffectsCalculatedPropertyInternalClassInternalProperty()
            {
                var before = @"
namespace N.Client
{
    internal class C : N.Core.ViewModelBase
    {
        private string p2;

        internal string P1 => $""Hello {this.p2}"";

        internal string P2
        {
            get { return this.p2; }
            set
            {
                this.TrySet(ref this.p2, value)
                ↓this.OnPropertyChanged(nameof(this.P1));
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    internal class C : N.Core.ViewModelBase
    {
        private string p2;

        internal string P1 => $""Hello {this.p2}"";

        internal string P2
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
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                }

                ↓this.OnPropertyChanged(nameof(this.P1));
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

        public string P1 => $""Hello {this.p2}"";

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
        private string p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (this.TrySet(ref this.p3, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }

                ↓this.OnPropertyChanged(nameof(this.P2));
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (this.TrySet(ref this.p3, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                    this.OnPropertyChanged(nameof(this.P2));
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
        private string p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (this.TrySet(ref this.p3, value))
                    this.OnPropertyChanged(nameof(this.P1));
                ↓this.OnPropertyChanged(nameof(this.P2));
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p3;

        public string P1 => $""Hello {this.p3}"";

        public string P2 => $""Hej {this.p3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (this.TrySet(ref this.p3, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                    this.OnPropertyChanged(nameof(this.P2));
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
