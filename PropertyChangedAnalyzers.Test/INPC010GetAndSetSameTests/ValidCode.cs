namespace PropertyChangedAnalyzers.Test.INPC010GetAndSetSameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = INPC010GetAndSetSame.Descriptor;

        [Test]
        public void NotifyingProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged(nameof(Value));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void WithBackingFieldExpressionBodies()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo
    {
        private int value;

        public int Value
        {
            get => this.value;
            set => this.value = value;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void NestedField()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.bar.BarValue;
            set
            {
                if (value == this.bar.BarValue)
                {
                    return;
                }

                this.bar.BarValue = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, barCode, testCode);
        }

        [Test]
        public void NestedProperties()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.bar.P2;
            set
            {
                if (value == this.bar.P2)
                {
                    return;
                }

                this.bar.P2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, barCode, testCode);
        }

        [Test]
        public void TimeSpanTicks()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WrappingPoint()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("get => Math.Abs(this.speed - 1) < 1E-2;")]
        [TestCase("get => Math.Abs(this.Speed - 1) < 1E-2;")]
        public void IsSpeed1(string getter)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ExplicitImplementationWithCast()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo<T> : IFoo
    {
        private T value;

        public T Value
        {
            get => this.value;
            set => this.value = value;
        }

        object IFoo.Value
        {
            get => this.value;
            set => this.Value = (T)value;
        }
    }

    interface IFoo
    {
        object Value { get; set; }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IntAndStringPropertyReturnFieldInGetter()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IntAndStringPropertyReturnPropertyInGetter()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IntPropertiesReturnFieldInGetter()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void IntPropertiesReturnPropertyInGetter()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void Issue102()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
