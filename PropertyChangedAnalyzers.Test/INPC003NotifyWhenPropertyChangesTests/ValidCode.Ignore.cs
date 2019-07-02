namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class Ignore
        {
            [Test]
            public static void Lazy1()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand
        {
            get
            {
                if (this.fooCommand != null)
                {
                    return this.fooCommand;
                }

                this.fooCommand = new DelegateCommand(param => this.Foo = true);
                return this.fooCommand;
            }
        }

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
                RoslynAssert.Valid(Analyzer, commandCode, testCode);
            }

            [Test]
            public static void Lazy2()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand
        {
            get
            {
                if (this.fooCommand == null)
                {
                    this.fooCommand = new DelegateCommand(param => this.Foo = true);
                }

                return this.fooCommand;
            }
        }

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                RoslynAssert.Valid(Analyzer, commandCode, testCode);
            }

            [Test]
            public static void LazyNullCoalesce()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand
        {
            get
            {
                return this.fooCommand ?? (this.fooCommand = new DelegateCommand(param => this.Foo = true));
            }
        }

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                RoslynAssert.Valid(Analyzer, commandCode, testCode);
            }

            [Test]
            public static void LazyNullCoalesceExpressionBody()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand => this.fooCommand ?? (this.fooCommand = new DelegateCommand(param => this.Foo = true));

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                RoslynAssert.Valid(Analyzer, commandCode, testCode);
            }

            [Test]
            public static void InCtor()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public ViewModel(string name)
        {
            this.name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void InInitializer()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        public ViewModel Create(string name)
        {
            return new ViewModel { name = name };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test(Description = "We let INPC002 nag about this.")]
            public static void SimplePropertyWithBackingField()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void AssigningFieldsInGetter()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;
        private int getCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                this.getCount++;
                return this.name;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void LazyGetter()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                if (this.name == null)
                {
                    this.name = string.Empty;
                }

                return this.name;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void LazyGetterExpressionBody()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name ?? (this.name = string.Empty);

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void DisposeMethod()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ViewModel : INotifyPropertyChanged, IDisposable
    {
        private string name;
        private bool disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                this.ThrowIfDisposed();
                return this.name ?? (this.name = string.Empty);
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public static void Recursive()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private double h1;

        public event PropertyChangedEventHandler PropertyChanged;

        public double H1
        {
            get => this.h1;
            set
            {
                if (value.Equals(this.h1))
                {
                    return;
                }

                this.h1 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Height));
            }
        }

        public double Height
        {
            get
            {
                return Math.Min(this.Height, this.H1);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.Valid(Analyzer, testCode);
            }
        }
    }
}
