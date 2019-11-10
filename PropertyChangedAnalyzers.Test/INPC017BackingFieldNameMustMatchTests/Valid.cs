namespace PropertyChangedAnalyzers.Test.INPC017BackingFieldNameMustMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC017BackingFieldNameMisMatch;

        [Test]
        public static void NotifyingProperty()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void ExpressionBody()
        {
            var code = @"
namespace N
{
    public class C
    {
        private int value;

        public int Value => this.value;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ExpressionBodyWhenKeyword()
        {
            var code = @"
namespace N
{
    public class C
    {
        private int @default;

        public int Default => this.@default;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WithBackingFieldExpressionBodies()
        {
            var code = @"
namespace N
{
    public class C
    {
        private int value;

        public int Value
        {
            get => this.value;
            set => this.value = value;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenSettingNestedField()
        {
            var barCode = @"
namespace N
{
    public class C1
    {
        public int C1Value;
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

        public int Value
        {
            get => this.c1.C1Value;
            set
            {
                if (value == this.c1.C1Value)
                {
                    return;
                }

                this.c1.C1Value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, barCode, code);
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

        [Test]
        public static void ExplicitImplementationWithCast()
        {
            var code = @"
namespace N
{
    public class C<T> : IC
    {
        private T value;

        public T Value
        {
            get => this.value;
            set => this.value = value;
        }

        object IC.Value
        {
            get => this.value;
            set => this.Value = (T)value;
        }
    }

    interface IC
    {
        object Value { get; set; }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void BackingFiledSum()
        {
            var code = @"
namespace N
{
    using System.Linq;

    public class C
    {
        private int[] ints;

        public int Sum => this.ints.Sum();
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void TextLength()
        {
            var code = @"
namespace N
{
    public class Foo
    {
        private string text;

        public int TextLength => this.text.Length;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenUsingMoreThanOneField()
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int value1;
        private int value2;
        private P p;

        public event PropertyChangedEventHandler PropertyChanged;

        public P P
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
                this.OnPropertyChanged(nameof(this.CurrentValue));
            }
        }

        public int CurrentValue
        {
            get
            {
                switch (P)
                {
                    case P.Value1:
                        return this.value1;
                    case P.Value2:
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

    public enum P
    {
        Value1,
        Value2,
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void StaticField()
        {
            var code = @"
namespace N
{
    public class C
    {
        private static readonly int StaticValue = 2;

        public int Value => StaticValue;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void StaticPropertyAndField()
        {
            var code = @"
namespace N
{
    public class C
    {
        private const int StaticValue = 2;

        public static int Value => StaticValue;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ConstField()
        {
            var code = @"
namespace N
{
    public class C
    {
        private const int ConstValue = 2;

        public int Value => ConstValue;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void SequenceOfUpperCase()
        {
            var code = @"
namespace N
{
    public class C
    {
        private string inpcEnabled;

        public string INPCEnabled
        {
            get => this.inpcEnabled;
            set => this.inpcEnabled = value;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void SequenceOfUpperCaseUnderscore()
        {
            var code = @"
namespace N
{
    public class C
    {
        private string _inpcEnabled;

        public string INPCEnabled
        {
            get => _inpcEnabled;
            set => _inpcEnabled = value;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
