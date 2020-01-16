namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class WhenNoCheck
        {
            [Test]
            public static void CallsOnPropertyChanged()
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

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                this.p = value;
                ↓this.OnPropertyChanged(nameof(P));
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
                this.OnPropertyChanged(nameof(P));
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
            public static void CallsOnPropertyChangedInternalClassInternalProperty()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int P
        {
            get
            {
                return this.p;
            }

            set
            {
                this.p = value;
                ↓this.OnPropertyChanged(nameof(P));
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

    internal class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int P
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
                this.OnPropertyChanged(nameof(P));
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
            public static void CallsRaisePropertyChangedWithEventArgs()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                this.p = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
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

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedInvokeElvis()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                this.p = value;
                ↓this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(P)));
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

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(P)));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedInvoke()
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

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                this.p = value;
                ↓this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.P)));
            }
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
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(this.P)));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedInvokeStaticClass()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public static class C
    {
        private static int p;

        public static event PropertyChangedEventHandler PropertyChanged;

        public static int P
        {
            get => p;
            set
            {
                p = value;
                ↓PropertyChanged.Invoke(null, new PropertyChangedEventArgs(nameof(P)));
            }
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;

    public static class C
    {
        private static int p;

        public static event PropertyChangedEventHandler PropertyChanged;

        public static int P
        {
            get => p;
            set
            {
                if (value == p)
                {
                    return;
                }

                p = value;
                PropertyChanged.Invoke(null, new PropertyChangedEventArgs(nameof(P)));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedInvokeUnderscore()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return _p;
            }

            set
            {
                _p = value;
                ↓PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(P)));
            }
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
        private int _p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return _p;
            }

            set
            {
                if (value == _p)
                {
                    return;
                }

                _p = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(P)));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedInvokeCachedEventArgs()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs ValuePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(P));
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                this.p = value;
                ↓this.PropertyChanged.Invoke(this, ValuePropertyChangedEventArgs);
            }
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
        private static readonly PropertyChangedEventArgs ValuePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(P));
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.PropertyChanged.Invoke(this, ValuePropertyChangedEventArgs);
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedInvokeNoNullCheck()
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

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                this.p = value;
                ↓this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(P)));
            }
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
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(P)));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void WhenSettingNestedField()
            {
                var c1 = @"
namespace N
{
    public class C1
    {
        public int F;
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.c1.F;
            set
            {
                this.c1.F = value;
                ↓this.OnPropertyChanged();
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
        private readonly C1 c1 = new C1();
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.c1.F;
            set
            {
                if (value == this.c1.F)
                {
                    return;
                }

                this.c1.F = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
            }

            [Test]
            public static void UseTrySet()
            {
                var before = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.OnPropertyChanged();
            }
        }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "TrySet(ref field, value)");
            }

            [TestCase("this.TrySet(ref this.p2, value)")]
            [TestCase("_ = this.TrySet(ref this.p2, value)")]
            public static void DiscardedTrySetThenOnPropertyChanged(string trySet)
            {
                var before = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => this.p2;
            set
            {
                this.TrySet(ref this.p2, value);
                ↓this.OnPropertyChanged(nameof(this.P1));
            }
        }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.TrySet(ref this.p2, value)", trySet);

                var after = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => this.p2;
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void UseTrySetNoCallerMemberName()
            {
                var before = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.OnPropertyChanged(nameof(this.P));
            }
        }

        protected bool TrySet<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
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

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value, nameof(this.P));
        }

        protected bool TrySet<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "TrySet(ref field, value, propertyName)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "TrySet(ref field, value, propertyName)");
            }

            [Test]
            public static void UseTrySetUnderscoreNoCallerMemberName()
            {
                var before = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private string _p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get => _p;
            set
            {
                _p = value;
                ↓OnPropertyChanged(nameof(P));
            }
        }

        protected bool TrySet<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private string _p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get => _p;
            set => TrySet(ref _p, value, nameof(P));
        }

        protected bool TrySet<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "TrySet(ref field, value, propertyName)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "TrySet(ref field, value, propertyName)");
            }

            [Test]
            public static void WarnOnlyOnFirstOnPropertyChanged()
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

        public int Squared => this.p * this.p;

        public int P
        {
            get => this.p;
            set
            {
                this.p = value;
                ↓this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Squared));
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

        public int Squared => this.p * this.p;

        public int P
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
                this.OnPropertyChanged(nameof(Squared));
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
        }
    }
}
