namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFixAll : CodeFixVerifier<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>
    {
        [Test]
        public void AutoPropertiesCallerMemberNameNameUnderscoreNames()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1 { get; set; }

        public int Bar2 { get; set; }

        public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
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
            get
            {
                return _bar1;
            }

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
            get
            {
                return _bar2;
            }

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
            get
            {
                return _bar3;
            }

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
            AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void AutoPropertiesCallerMemberNameNameUnderscoreNamesWithExistingNotifying1()
        {
            var testCode = @"
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
            get
            {
                return _bar1;
            }

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

        public int Bar2 { get; set; }

        public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
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
            get
            {
                return _bar1;
            }

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
            get
            {
                return _bar2;
            }

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
            get
            {
                return _bar3;
            }

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
            AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void AutoPropertiesCallerMemberNameNameUnderscoreNamesWithExistingNotifying2()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1 { get; set; }

        public int Bar2
        {
            get
            {
                return _bar2;
            }

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

        public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
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
            get
            {
                return _bar1;
            }

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
            get
            {
                return _bar2;
            }

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
            get
            {
                return _bar3;
            }

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
            AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void AutoPropertiesCallerMemberNameNameUnderscoreNamesWithExistingNotifying3()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1 { get; set; }

        public int Bar2 { get; set; }

        public int Bar3
        {
            get
            {
                return _bar3;
            }

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

            var fixedCode = @"
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
            get
            {
                return _bar1;
            }

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
            get
            {
                return _bar2;
            }

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
            get
            {
                return _bar3;
            }

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
            AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void AutoPropertiesCallerMemberNameNameUnderscoreNamesTwoClassesInDocument()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1 { get; set; }

        public int Bar2 { get; set; }

        public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Foo2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1 { get; set; }

        public int Bar2 { get; set; }

        public int Bar3 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo1 : INotifyPropertyChanged
    {
        private int _bar1;
        private int _bar2;
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get
            {
                return _bar1;
            }

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
            get
            {
                return _bar2;
            }

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
            get
            {
                return _bar3;
            }

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

    public class Foo2 : INotifyPropertyChanged
    {
        private int _bar1;
        private int _bar2;
        private int _bar3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar1
        {
            get
            {
                return _bar1;
            }

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
            get
            {
                return _bar2;
            }

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
            get
            {
                return _bar3;
            }

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
            AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode);
        }
    }
}