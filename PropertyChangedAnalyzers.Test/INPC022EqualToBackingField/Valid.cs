namespace PropertyChangedAnalyzers.Test.INPC022EqualToBackingField
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC022EqualToBackingField;

        [Test]
        public static void NotifyingProperty()
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

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WithBackingFieldStatementBodies()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WithBackingFieldStatementBodiesAssigningTwice()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set
            { 
                this.p = value;
                this.p = value;
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void WithBackingFieldExpressionBodies()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private int p;

        public int P
        {
            get => this.p;
            set => this.p = value;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void NestedField()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int p;
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.c1.p;
            set
            {
                if (value == this.c1.p)
                {
                    return;
                }

                this.c1.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, c1, code);
        }

        [Test]
        public static void NestedProperties()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.c1.P2;
            set
            {
                if (value == this.c1.P2)
                {
                    return;
                }

                this.c1.P2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, c1, code);
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

        [TestCase("get => Math.Abs(this.speed - 1) < 1E-2;")]
        [TestCase("get => Math.Abs(this.Speed - 1) < 1E-2;")]
        public static void IsSpeed1(string getter)
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private double speed;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSpeed1
        {
            get => Math.Abs(this.speed - 1) < 1E-2;
            set => this.Speed = 1;
        }

        public double Speed
        {
            get => this.speed;

            set
            {
                if (value.Equals(this.speed))
                {
                    return;
                }

                this.speed = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsSpeed1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("get => Math.Abs(this.speed - 1) < 1E-2;", getter);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ExplicitImplementationWithCast()
        {
            var code = @"
namespace N
{
    public class C<T> : I
    {
        private T p;

        public T P
        {
            get => this.p;
            set => this.p = value;
        }

        object I.P
        {
            get => this.p;
            set => this.P = (T)value;
        }
    }

    interface I
    {
        object P { get; set; }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IntAndStringPropertyReturnFieldInGetter()
        {
            var code = @"
namespace ValidCode
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public class IntAndStringProperty : INotifyPropertyChanged
    {
        private int p1;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        public string P2
        {
            get => this.p1.ToString(CultureInfo.InvariantCulture);
            set => this.P1 = int.Parse(value, CultureInfo.InvariantCulture);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IntAndStringPropertyReturnPropertyInGetter()
        {
            var code = @"
namespace ValidCode
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public class IntAndStringProperty : INotifyPropertyChanged
    {
        private int p1;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        public string P2
        {
            get => this.P1.ToString(CultureInfo.InvariantCulture);
            set => this.P1 = int.Parse(value, CultureInfo.InvariantCulture);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IntPropertiesAssignsPropertyReturnField()
        {
            var code = @"
namespace ValidCode
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p1;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        public int P2
        {
            get => this.p1;
            set => this.P1 = value;
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
        public static void IntPropertiesAssignsFiledReturnsProperty()
        {
            var code = @"
namespace ValidCode
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p1;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        public int P2
        {
            get => this.P1;
            set => this.p1 = value;
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
        public static void IntPropertiesReturnPropertyInGetter()
        {
            var code = @"
namespace ValidCode
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p1;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        public int P2
        {
            get => this.P1;
            set => this.P1 = value;
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
        public static void Issue102()
        {
            var code = @"
namespace ValidCode.Repros
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public interface IC
    {
        INotifyPropertyChanged Source { get; set; }
    }

    public sealed class C<T> : INotifyPropertyChanged, IDisposable, IC
         where T : class, INotifyPropertyChanged
    {
        private readonly PropertyChangedEventHandler onTrackedPropertyChanged;
        private readonly object gate = new object();

        private T source;
        private bool disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        public T Source
        {
            get => this.source;

            set
            {
                if (this.disposed)
                {
                    return;
                }

                lock (this.gate)
                {
                    if (this.disposed ||
                        ReferenceEquals(value, this.source))
                    {
                        return;
                    }

                    if (this.source != null)
                    {
                        this.source.PropertyChanged -= this.onTrackedPropertyChanged;
                    }

                    if (value != null)
                    {
                        value.PropertyChanged += this.onTrackedPropertyChanged;
                    }

                    this.source = value;
                    this.OnPropertyChanged();
                }
            }
        }

        INotifyPropertyChanged IC.Source
        {
            get => this.source;
            set => this.Source = (T)value;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            INotifyPropertyChanged oldSource;
            lock (this.gate)
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;
                oldSource = this.source;
                this.source = null;
            }

            if (oldSource != null)
            {
                oldSource.PropertyChanged -= this.onTrackedPropertyChanged;
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void TrySet()
        {
            var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int f1;
        private int f2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.f1;
            set => this.TrySet(ref this.f1, value);
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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void IgnoreWhenNotGettingSame()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.f;
            }

            set
            {
                if (value == this.f)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(P));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
