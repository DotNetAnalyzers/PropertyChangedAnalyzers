namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class Ignore
        {
            [Test]
            public static void Lazy1()
            {
                var delegateCommand = @"
namespace N
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
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private bool p;
        private DelegateCommand command;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand Command
        {
            get
            {
                if (this.command != null)
                {
                    return this.command;
                }

                this.command = new DelegateCommand(param => this.P = true);
                return this.command;
            }
        }

        public bool P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (this.p != value)
                {
                    this.p = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
                RoslynAssert.Valid(Analyzer, delegateCommand, code);
            }

            [Test]
            public static void Lazy2()
            {
                var delegateCommand = @"
namespace N
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
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private bool p;
        private DelegateCommand command;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand Command
        {
            get
            {
                if (this.command == null)
                {
                    this.command = new DelegateCommand(param => this.P = true);
                }

                return this.command;
            }
        }

        public bool P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (this.p != value)
                {
                    this.p = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                RoslynAssert.Valid(Analyzer, delegateCommand, code);
            }

            [Test]
            public static void LazyNullCoalesce()
            {
                var delegateCommand = @"
namespace N
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
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private bool p2;
        private DelegateCommand p1;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand P1
        {
            get
            {
                return this.p1 ?? (this.p1= new DelegateCommand(param => this.P2 = true));
            }
        }

        public bool P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (this.p2 != value)
                {
                    this.p2 = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                RoslynAssert.Valid(Analyzer, delegateCommand, code);
            }

            [Test]
            public static void LazyNullCoalesceExpressionBody()
            {
                var delegateCommand = @"
namespace N
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
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private bool p;
        private DelegateCommand pCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand PCommand => this.pCommand ?? (this.pCommand = new DelegateCommand(param => this.P = true));

        public bool P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (this.p != value)
                {
                    this.p = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                RoslynAssert.Valid(Analyzer, delegateCommand, code);
            }

            [Test]
            public static void InCtor()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public C(string p)
        {
            this.p = p;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void InInitializer()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P => this.p;

        public C Create(string p)
        {
            return new C { p = p };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test(Description = "We let INPC002 nag about this.")]
            public static void SimplePropertyWithBackingField()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void AssigningFieldsInGetter()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;
        private int getCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get
            {
                this.getCount++;
                return this.p;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void LazyGetter()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get
            {
                if (this.p == null)
                {
                    this.p = string.Empty;
                }

                return this.p;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void LazyGetterExpressionBody()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P => this.p ?? (this.p = string.Empty);

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void DisposeMethod()
            {
                var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged, IDisposable
    {
        private string p;
        private bool disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get
            {
                this.ThrowIfDisposed();
                return this.p ?? (this.p = string.Empty);
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

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void Recursive()
            {
                var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
                RoslynAssert.Valid(Analyzer, code);
            }
        }
    }
}
