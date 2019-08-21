namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFixAll
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new MakePropertyNotifyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC002MutablePublicPropertyShouldNotify);

        [Test]
        public static void TwoAutoPropertiesCallerMemberName()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        ↓public int Bar1 { get; set; }

        ↓public int Bar2 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly int value;
        private int bar1;
        private int bar2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        public int Bar1
        {
            get => this.bar1;
            set
            {
                if (value == this.bar1)
                {
                    return;
                }

                this.bar1 = value;
                this.OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => this.bar2;
            set
            {
                if (value == this.bar2)
                {
                    return;
                }

                this.bar2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        ↓public int Bar1 { get; set; }

        ↓public int Bar2 { get; set; }

        ↓public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly int value;
        private int bar1;
        private int bar2;
        private int bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        public int Bar1
        {
            get => this.bar1;
            set
            {
                if (value == this.bar1)
                {
                    return;
                }

                this.bar1 = value;
                this.OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => this.bar2;
            set
            {
                if (value == this.bar2)
                {
                    return;
                }

                this.bar2 = value;
                this.OnPropertyChanged();
            }
        }

        public int Bar3
        {
            get => this.bar3;
            set
            {
                if (value == this.bar3)
                {
                    return;
                }

                this.bar3 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => _value;

        ↓public int Bar1 { get; set; }

        ↓public int Bar2 { get; set; }

        ↓public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly int _value;
        private int _bar1;
        private int _bar2;
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => _value;

        public int Bar1
        {
            get => _bar1;
            set
            {
                if (value == _bar1)
                {
                    return;
                }

                _bar1 = value;
                OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => _bar2;
            set
            {
                if (value == _bar2)
                {
                    return;
                }

                _bar2 = value;
                OnPropertyChanged();
            }
        }

        public int Bar3
        {
            get => _bar3;
            set
            {
                if (value == _bar3)
                {
                    return;
                }

                _bar3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after);
        }

        [Test]
        public static void AutoPropertiesCallerMemberNameUnderscoreNamesWithExistingNotifying1()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar1;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get => _bar1;
            set
            {
                if (value == _bar1)
                {
                    return;
                }

                _bar1 = value;
                OnPropertyChanged();
            }
        }

        ↓public int Bar2 { get; set; }

        ↓public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar1;
        private int _bar2;
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get => _bar1;
            set
            {
                if (value == _bar1)
                {
                    return;
                }

                _bar1 = value;
                OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => _bar2;
            set
            {
                if (value == _bar2)
                {
                    return;
                }

                _bar2 = value;
                OnPropertyChanged();
            }
        }

        public int Bar3
        {
            get => _bar3;
            set
            {
                if (value == _bar3)
                {
                    return;
                }

                _bar3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar2;

        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Bar1 { get; set; }

        public int Bar2
        {
            get => _bar2;
            set
            {
                if (value == _bar2)
                {
                    return;
                }

                _bar2 = value;
                OnPropertyChanged();
            }
        }

        ↓public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar1;
        private int _bar2;
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get => _bar1;
            set
            {
                if (value == _bar1)
                {
                    return;
                }

                _bar1 = value;
                OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => _bar2;
            set
            {
                if (value == _bar2)
                {
                    return;
                }

                _bar2 = value;
                OnPropertyChanged();
            }
        }

        public int Bar3
        {
            get => _bar3;
            set
            {
                if (value == _bar3)
                {
                    return;
                }

                _bar3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Bar1 { get; set; }

        ↓public int Bar2 { get; set; }

        public int Bar3
        {
            get => _bar3;
            set
            {
                if (value == _bar3)
                {
                    return;
                }

                _bar3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar1;
        private int _bar2;
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get => _bar1;
            set
            {
                if (value == _bar1)
                {
                    return;
                }

                _bar1 = value;
                OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => _bar2;
            set
            {
                if (value == _bar2)
                {
                    return;
                }

                _bar2 = value;
                OnPropertyChanged();
            }
        }

        public int Bar3
        {
            get => _bar3;
            set
            {
                if (value == _bar3)
                {
                    return;
                }

                _bar3 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Bar1 { get; set; }

        ↓public int Bar2 { get; set; }

        ↓public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Foo2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Bar1 { get; set; }

        ↓public int Bar2 { get; set; }

        ↓public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo1 : INotifyPropertyChanged
    {
        private int bar1;
        private int bar2;
        private int bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get => this.bar1;
            set
            {
                if (value == this.bar1)
                {
                    return;
                }

                this.bar1 = value;
                this.OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => this.bar2;
            set
            {
                if (value == this.bar2)
                {
                    return;
                }

                this.bar2 = value;
                this.OnPropertyChanged();
            }
        }

        public int Bar3
        {
            get => this.bar3;
            set
            {
                if (value == this.bar3)
                {
                    return;
                }

                this.bar3 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Foo2 : INotifyPropertyChanged
    {
        private int bar1;
        private int bar2;
        private int bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get => this.bar1;
            set
            {
                if (value == this.bar1)
                {
                    return;
                }

                this.bar1 = value;
                this.OnPropertyChanged();
            }
        }

        public int Bar2
        {
            get => this.bar2;
            set
            {
                if (value == this.bar2)
                {
                    return;
                }

                this.bar2 = value;
                this.OnPropertyChanged();
            }
        }

        public int Bar3
        {
            get => this.bar3;
            set
            {
                if (value == this.bar3)
                {
                    return;
                }

                this.bar3 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
