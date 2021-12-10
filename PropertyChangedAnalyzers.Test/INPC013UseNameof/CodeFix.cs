namespace PropertyChangedAnalyzers.Test.INPC013UseNameof
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly ArgumentAnalyzer Analyzer = new();
        private static readonly UseNameofFix Fix = new UseNameofFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC013UseNameof);

        [Test]
        public static void OnPropertyChanged()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 => this.P2 * this.P2;

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(↓""P2"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 => this.P2 * this.P2;

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void OnPropertyChangedUnderscoreNames()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p2;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
        }

        [Test]
        public static void PropertyChangedInvoke()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(↓""P""));
            }
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.P)));
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void StaticEventHandlerOfPropertyChangedEventArgsInvoke()
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
            get => p;
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
            get => p;
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
        public static void StaticPropertyChangedEventHandlerInvoke()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C
    {
        private static string p;

        public static event PropertyChangedEventHandler? PropertyChanged;

        public static string P
        {
            get => p;
            set
            {
                if (value == p)
                {
                    return;
                }

                p = value;
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(↓""P""));
            }
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public class C
    {
        private static string p;

        public static event PropertyChangedEventHandler? PropertyChanged;

        public static string P
        {
            get => p;
            set
            {
                if (value == p)
                {
                    return;
                }

                p = value;
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(P)));
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void StaticPropertyChangedEventHandlerInvoker()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C
    {
        private static string p;

        public static event PropertyChangedEventHandler? PropertyChanged;

        public static string P
        {
            get => p;
            set
            {
                if (value == p)
                {
                    return;
                }

                p = value;
                OnPropertyChanged(↓""P"");
            }
        }

        private static void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public class C
    {
        private static string p;

        public static event PropertyChangedEventHandler? PropertyChanged;

        public static string P
        {
            get => p;
            set
            {
                if (value == p)
                {
                    return;
                }

                p = value;
                OnPropertyChanged(nameof(P));
            }
        }

        private static void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
