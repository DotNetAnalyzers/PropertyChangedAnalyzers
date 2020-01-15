namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC005CheckIfDifferentBeforeNotifying;

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
        {
            new TestCaseData("string", "Equals(value, this.p)"),
            new TestCaseData("string", "Equals(this.p, value)"),
            new TestCaseData("string", "Equals(value, p)"),
            new TestCaseData("string", "Equals(value, P)"),
            new TestCaseData("string", "Equals(P, value)"),
            new TestCaseData("string", "object.Equals(P, value)"),
            new TestCaseData("string", "Object.Equals(P, value)"),
            new TestCaseData("string", "System.Object.Equals(P, value)"),
            new TestCaseData("string", "Nullable.Equals(value, this.p)"),
            new TestCaseData("int?",   "Nullable.Equals(value, this.p)"),
            new TestCaseData("int?",   "System.Nullable.Equals(value, this.p)"),
            new TestCaseData("string", "value.Equals(this.p)"),
            new TestCaseData("string", "value.Equals(p)"),
            new TestCaseData("string", "this.p.Equals(value)"),
            new TestCaseData("string", "p.Equals(value)"),
            new TestCaseData("string", "string.Equals(value, this.p, StringComparison.OrdinalIgnoreCase)"),
            new TestCaseData("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.p)"),
            new TestCaseData("string", "ReferenceEquals(value, this.p)"),
            new TestCaseData("string", "object.ReferenceEquals(value, this.p)"),
            new TestCaseData("string", "Object.ReferenceEquals(value, this.p)"),
            new TestCaseData("string", "System.Object.ReferenceEquals(value, this.p)"),
        };

        [TestCaseSource(nameof(TestCases))]
        public static void Check(string type, string expression)
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.p)", expression)
  .AssertReplace("int", type);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void NegatedCheck(string type, string expression)
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (!Equals(value, this.p))
                {
                    this.p = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.p)", expression)
  .AssertReplace("int", type);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void SimplePropertyBlockBodies()
        {
            var code = @"
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
            get { return this.p; }
            set { this.p = value; }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void SimplePropertyExpressionBodies()
        {
            var code = @"
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
            get => this.p;
            set => this.p = value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void IfValueEqualsFieldReturnElseAssignAndNotify()
        {
            var code = @"
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
            get => this.p;
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged();
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
        public static void IfValueEqualsPropertyReturnElseAssignAndNotify()
        {
            var code = @"
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
            get => this.p;
            set
            {
                if (value == this.P) return;
                this.p = value;
                this.OnPropertyChanged();
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
        public static void IfValueNotEqualsFieldAssignAndNotifyCallerMemberName()
        {
            var code = @"
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
            get => this.p;
            set
            {
                if (value != this.p)
                {
                    this.p = value;
                    this.OnPropertyChanged();
                }
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
        public static void IfValueNotEqualsFieldAssignAndNotifyPropertyChangedEventArgs()
        {
            var code = @"
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
            get => this.p;
            set
            {
                if (value != this.p)
                {
                    this.p = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedCallerMemberName()
        {
            var code = @"
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
            get => this.p;
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged();
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
        public static void Invokes()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;
        public event PropertyChangedEventHandler PropertyChanged;

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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.P)));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void InvokesCached()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PPropertyChangedArgs = new PropertyChangedEventArgs(nameof(P));
 
        private int p;
        
        public event PropertyChangedEventHandler PropertyChanged;

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
                this.PropertyChanged?.Invoke(this, PPropertyChangedArgs);
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void CheckSideEffectReturn()
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;
        private int misses;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P
        {
            get => this.p;
            set
            {
                if (Equals(value, this.p))
                {
                    misses++;
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
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
        public static void WithCheckAndThrowBefore()
        {
            var code = @"
namespace N
{
    using System;

    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isBusy;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool IsBusy
        {
            get => _isBusy;

            private set
            {
                if (value && _isBusy)
                {
                    throw new InvalidOperationException(""Already busy"");
                }

                if (value == _isBusy)
                {
                    return;
                }

                _isBusy = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("if (Math.Abs(value - this.p) < 1e-6)")]
        [TestCase("if (Math.Abs(this.p - value) < 1e-6)")]
        public static void WithCheckAndThrowBefore(string expression)
        {
            var code = @"
namespace N
{
    using System;

    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private double p;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public double P
        {
            get => p;

            set
            {
                if (Math.Abs(value - this.p) < 1e-6)
                {
                    return;
                }

                this.p = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("if (Math.Abs(value - this.p) < 1e-6)", expression);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void CheckInLock()
        {
            var code = @"
namespace N
{
    using System;

    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly object _busyLock = new object();
        private bool _p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool Value
        {
            get => _p;
            private set
            {
                lock (_busyLock)
                {
                    if (value && _p)
                    {
                        throw new InvalidOperationException();
                    }

                    if (value == _p)
                    {
                        return;
                    }

                    _p = value;
                }

                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void CheckLockCheck()
        {
            var code = @"
namespace N
{
    using System;

    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly object _gate = new object();
        private object _p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public object Value
        {
            get => _p;
            private set
            {
                if (ReferenceEquals(value, _p))
                {
                    return;
                }

                lock (_gate)
                {
                    if (ReferenceEquals(value, _p))
                    {
                        return;
                    }

                    _p = value;
                }

                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenSettingNestedField()
        {
            var c2 = @"
namespace N
{
    public class C1
    {
        public int F;
    }
}";
            var code = @"
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
            RoslynAssert.Valid(Analyzer, c2, code);
        }

        [Test]
        public static void WrappingPoint()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private Point point;

        public event PropertyChangedEventHandler PropertyChanged;

        public int X
        {
            get => this.point.X;
            set
            {
                if (value == this.point.X)
                {
                    return;
                }

                this.point = new Point(value, this.point.Y);
                this.OnPropertyChanged();
            }
        }

        public int Y
        {
            get => this.point.Y;
            set
            {
                if (value == this.point.Y)
                {
                    return;
                }

                this.point = new Point(this.point.X, value);
                this.OnPropertyChanged();
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
        public static void TimeSpanTicks()
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private TimeSpan timeSpan;

        public event PropertyChangedEventHandler PropertyChanged;

        public long Ticks
        {
            get => this.timeSpan.Ticks;
            set
            {
                if (value == this.timeSpan.Ticks)
                {
                    return;
                }

                this.timeSpan = TimeSpan.FromTicks(value);
                this.OnPropertyChanged();
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
    }
}
