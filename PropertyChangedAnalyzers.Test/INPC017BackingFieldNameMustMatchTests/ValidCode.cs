namespace PropertyChangedAnalyzers.Test.INPC017BackingFieldNameMustMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = INPC017BackingFieldNameMustMatch.Descriptor;

        [Test]
        public static void NotifyingProperty()
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

            RoslynAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public static void ExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value;

        public int Value => this.value;
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void ExpressionBodyWhenKeyword()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int @default;

        public int Default => this.@default;
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WithBackingFieldExpressionBodies()
        {
            var testCode = @"
namespace RoslynSandbox
{
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

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenSettingNestedField()
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
            RoslynAssert.Valid(Analyzer, barCode, testCode);
        }

        [Test]
        public static void TimeSpanTicks()
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

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WrappingPoint()
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

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void ExplicitImplementationWithCast()
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

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void BackingFiledSum()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Linq;

    public class Foo
    {
        private int[] ints;

        public int Sum => this.ints.Sum();
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void TextLength()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private string text;

        public int TextLength => this.text.Length;
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenUsingMoreThanOneField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int value1;
        private int value2;
        private Baz baz;

        public event PropertyChangedEventHandler PropertyChanged;

        public Baz Baz
        {
            get => this.baz;
            set
            {
                if (value == this.baz)
                {
                    return;
                }

                this.baz = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CurrentValue));
            }
        }

        public int CurrentValue
        {
            get
            {
                switch (Baz)
                {
                    case Baz.Value1:
                        return this.value1;
                    case Baz.Value2:
                        return this.value2;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum Baz
    {
        Value1,
        Value2,
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void StaticField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private static readonly int StaticValue = 2;

        public int Value => StaticValue;
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void StaticPropertyAndField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private const int StaticValue = 2;

        public static int Value => StaticValue;
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void ConstField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private const int ConstValue = 2;

        public int Value => ConstValue;
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void SequenceOfUpperCase()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private string inpcEnabled;

        public string INPCEnabled
        {
            get => this.inpcEnabled;
            set => this.inpcEnabled = value;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void SequenceOfUpperCaseUnderscore()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private string _inpcEnabled;

        public string INPCEnabled
        {
            get => _inpcEnabled;
            set => _inpcEnabled = value;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
