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
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.Value*this.Value;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
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
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.Value*this.Value;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
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
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(↓""Name""));
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
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Name)));
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
        private static string p;
        private int value;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string P
        {
            get
            {
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(↓""Name""));
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
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
        private static string p;
        private int value;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static string P
        {
            get
            {
                return name;
            }

            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
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
        public int Value { get; set; }

        public static void M1()
        {
            M1(↓""Value"");
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
        public int Value { get; set; }

        public static void M1()
        {
            M1(nameof(Value));
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
        public static readonly string P = M(↓""Value"");

        public int Value { get; set; }

        public static string M(string s) => s;
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        public static readonly string P = M(nameof(Value));

        public int Value { get; set; }

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
        public readonly string P = string.Format(↓""Value"");

        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        public readonly string P = string.Format(nameof(Value));

        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
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
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => _value*_value;

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(↓""Squared"");
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
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => _value*_value;

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Squared));
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
