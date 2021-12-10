namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class FixAll
    {
        private static readonly SetAccessorAnalyzer Analyzer = new();
        private static readonly MakePropertyNotifyFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC002MutablePublicPropertyShouldNotify);

        [Test]
        public static void TwoAutoPropertiesCallerMemberName()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly int p = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public int ↓P1 { get; set; }

        public int ↓P2 { get; set; }

        public int M() => this.p;

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
        private readonly int p = 1;
        private int p1;
        private int p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

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
            }
        }

        public int M() => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertiesCallerMemberName()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly int p = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public int ↓P1 { get; set; }

        public int ↓P2 { get; set; }

        public int ↓P3 { get; set; }

        public int M() => this.p;

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
        private readonly int p = 1;
        private int p1;
        private int p2;
        private int p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

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
            }
        }

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
            }
        }

        public int M() => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertiesCallerMemberNameUnderscoreNames()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly int _p = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => _p;

        public int ↓P1 { get; set; }

        public int ↓P2 { get; set; }

        public int ↓P3 { get; set; }

        public int M() => _p;

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
        private readonly int _p = 1;
        private int _p1;
        private int _p2;
        private int _p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => _p;

        public int P1
        {
            get => _p1;
            set
            {
                if (value == _p1)
                {
                    return;
                }

                _p1 = value;
                OnPropertyChanged();
            }
        }

        public int P2
        {
            get => _p2;
            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged();
            }
        }

        public int P3
        {
            get => _p3;
            set
            {
                if (value == _p3)
                {
                    return;
                }

                _p3 = value;
                OnPropertyChanged();
            }
        }

        public int M() => _p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
        }

        [Test]
        public static void AutoPropertiesCallerMemberNameUnderscoreNamesWithExistingNotifying1()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => _p1;
            set
            {
                if (value == _p1)
                {
                    return;
                }

                _p1 = value;
                OnPropertyChanged();
            }
        }

        public int ↓P2 { get; set; }

        public int ↓P3 { get; set; }

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
        private int _p1;
        private int _p2;
        private int _p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => _p1;
            set
            {
                if (value == _p1)
                {
                    return;
                }

                _p1 = value;
                OnPropertyChanged();
            }
        }

        public int P2
        {
            get => _p2;
            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged();
            }
        }

        public int P3
        {
            get => _p3;
            set
            {
                if (value == _p3)
                {
                    return;
                }

                _p3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertiesCallerMemberNameUnderscoreNamesWithExistingNotifying2()
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

        public int ↓P1 { get; set; }

        public int P2
        {
            get => _p2;
            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged();
            }
        }

        public int ↓P3 { get; set; }

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
        private int _p1;
        private int _p2;
        private int _p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => _p1;
            set
            {
                if (value == _p1)
                {
                    return;
                }

                _p1 = value;
                OnPropertyChanged();
            }
        }

        public int P2
        {
            get => _p2;
            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged();
            }
        }

        public int P3
        {
            get => _p3;
            set
            {
                if (value == _p3)
                {
                    return;
                }

                _p3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertiesCallerMemberNameUnderscoreNamesWithExistingNotifying3()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P1 { get; set; }

        public int ↓P2 { get; set; }

        public int P3
        {
            get => _p3;
            set
            {
                if (value == _p3)
                {
                    return;
                }

                _p3 = value;
                OnPropertyChanged();
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
        private int _p1;
        private int _p2;
        private int _p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => _p1;
            set
            {
                if (value == _p1)
                {
                    return;
                }

                _p1 = value;
                OnPropertyChanged();
            }
        }

        public int P2
        {
            get => _p2;
            set
            {
                if (value == _p2)
                {
                    return;
                }

                _p2 = value;
                OnPropertyChanged();
            }
        }

        public int P3
        {
            get => _p3;
            set
            {
                if (value == _p3)
                {
                    return;
                }

                _p3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertiesCallerMemberNameUnderscoreNamesTwoClassesInDocument()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P1 { get; set; }

        public int ↓P2 { get; set; }

        public int ↓P3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class C2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P1 { get; set; }

        public int ↓P2 { get; set; }

        public int ↓P3 { get; set; }

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

    public class C1 : INotifyPropertyChanged
    {
        private int p1;
        private int p2;
        private int p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

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
            }
        }

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class C2 : INotifyPropertyChanged
    {
        private int p1;
        private int p2;
        private int p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

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
            }
        }

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
