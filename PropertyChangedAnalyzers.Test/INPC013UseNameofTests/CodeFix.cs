namespace PropertyChangedAnalyzers.Test.INPC013UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();
        private static readonly UseNameofFix Fix = new UseNameofFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC013UseNameof);

        [Test]
        public static void WhenThrowingArgumentException()
        {
            var before = @"
namespace N
{
    using System;

    public class C
    {
        public void M(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(↓""value"");
            }
        }
    }
}";

            var after = @"
namespace N
{
    using System;

    public class C
    {
        public void M(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenThrowingArgumentOutOfRangeException()
        {
            var before = @"
namespace N
{
    using System;

    public class C
    {
        public void M(StringComparison value)
        {
            switch (value)
            {
                default:
                    throw new ArgumentOutOfRangeException(↓""value"", value, null);
            }
        }
    }
}";

            var after = @"
namespace N
{
    using System;

    public class C
    {
        public void M(StringComparison value)
        {
            switch (value)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenRaisingPropertyChanged()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.P*this.P;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(↓""Squared"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.P*this.P;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Squared));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenRaisingStaticPropertyChanged()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;

    public static class C
    {
        private static string p;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string P
        {
            get
            {
                return p;
            }

            set
            {
                if (value == p)
                {
                    return;
                }

                p = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(↓""P""));
            }
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;

    public static class C
    {
        private static string p;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string P
        {
            get
            {
                return p;
            }

            set
            {
                if (value == p)
                {
                    return;
                }

                p = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(P)));
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenRaisingStaticPropertyChanged2()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;

    public class C
    {
        private static string p1;
        private int p2;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string P1
        {
            get
            {
                return p1;
            }

            set
            {
                if (value == p1)
                {
                    return;
                }

                p1 = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(↓""P1""));
            }
        }

        public int P2
        {
            get
            {
                return this.p2;
            }
            set
            {
                this.p2 = value;
            }
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;

    public class C
    {
        private static string p1;
        private int p2;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string P1
        {
            get
            {
                return p1;
            }

            set
            {
                if (value == p1)
                {
                    return;
                }

                p1 = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(P1)));
            }
        }

        public int P2
        {
            get
            {
                return this.p2;
            }
            set
            {
                this.p2 = value;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenStaticContextNameofInstance()
        {
            var before = @"
namespace N
{
    public class C
    {
        public int P { get; set; }

        public static void M1()
        {
            M1(↓""P"");
        }

        public static void M1(string s)
        {
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        public int P { get; set; }

        public static void M1()
        {
            M1(nameof(P));
        }

        public static void M1(string s)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenStaticContextNameofInstance2()
        {
            var before = @"
namespace N
{
    public class C
    {
        public static readonly string F = M(↓""P"");

        public int P { get; set; }

        public static string M(string s) => s;
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        public static readonly string F = M(nameof(P));

        public int P { get; set; }

        public static string M(string s) => s;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenStaticContextNameofInstance3()
        {
            var before = @"
namespace N
{
    public class C
    {
        public readonly string F = string.Format(↓""P"");

        private int p;

        public int P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        public readonly string F = string.Format(nameof(P));

        private int p;

        public int P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenRaisingPropertyChangedUnderscoreNames()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => _p2 * _p2;

        public int P2
        {
            get
            {
                return _p2;
            }

            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged();
                OnPropertyChanged(↓""P1"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => _p2 * _p2;

        public int P2
        {
            get
            {
                return _p2;
            }

            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
        }

        [Test]
        public static void DependencyProperty()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class WpfControl : Control
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            ↓""Number"",
            typeof(int),
            typeof(WpfControl),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get { return (int)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class WpfControl : Control
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(WpfControl),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get { return (int)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
