namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        [Test]
        public static void AddAfterLastOnPropertyChanged()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p3 * this.p3;

        public int P2 => this.p3 + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

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

    public class C : INotifyPropertyChanged
    {
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p3 * this.p3;

        public int P2 => this.p3 + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Explicit("Fix.")]
        [Test]
        public static void AddAfterOtherFieldAssignment()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f * this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                ↓this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

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

    public class C : INotifyPropertyChanged
    {
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f * this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
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
        public static void AddBeforeSideEffect()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p3 * this.p3;

        public int P2 => this.p3 + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.ToString();
            }
        }

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

    public class C : INotifyPropertyChanged
    {
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p3 * this.p3;

        public int P2 => this.p3 + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
                this.ToString();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AddBeforeSideEffectUnderscore()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => _p3 * _p3;

        public int P2 => _p3 + _p3;

        public int P3
        {
            get => _p3;
            set
            {
                if (value == _p3)
                {
                    return;
                }

                ↓_p3 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(P1));
                M();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void M()
        {
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => _p3 * _p3;

        public int P2 => _p3 + _p3;

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
                OnPropertyChanged(nameof(P1));
                OnPropertyChanged(nameof(P2));
                M();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void M()
        {
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Explicit("Fix.")]
        [Test]
        public static void IfTrySetBlockBody()
        {
            var before = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        public int P
        {
            get => this.p;
            set
            {
                if (this.TrySet(↓ref this.p, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        public int P
        {
            get => this.p;
            set
            {
                if (this.TrySet(↓ref this.p, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                    this.OnPropertyChanged(nameof(this.P2));
                }
            }
        }

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Explicit("Fix.")]
        [Test]
        public static void IfTrySetStatementBody()
        {
            var before = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        public int P
        {
            get => this.p;
            set
            {
                if (this.TrySet(↓ref this.p, value))
                    this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        public int P
        {
            get => this.p;
            set
            {
                if (this.TrySet(↓ref this.p, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                    this.OnPropertyChanged(nameof(this.P2));
                }
            }
        }

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Explicit("Fix.")]
        [Test]
        public static void IfNotTrySet()
        {
            var before = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        public int P
        {
            get => this.p;
            set
            {
                if (!this.TrySet(↓ref this.p, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        public int P
        {
            get => this.p;
            set
            {
                if (!this.TrySet(ref this.p, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
