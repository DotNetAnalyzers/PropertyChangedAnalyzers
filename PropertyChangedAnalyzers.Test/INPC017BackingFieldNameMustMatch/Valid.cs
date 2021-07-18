namespace PropertyChangedAnalyzers.Test.INPC017BackingFieldNameMustMatch
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
        public static void ExpressionBody()
        {
            var code = @"
namespace N
{
    public class C
    {
        private int p;

        public int P => this.p;
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
        public static void BackingFieldSum()
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
    public class C
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
        private int f1;
        private int f2;
        private E p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get
            {
                switch (this.p2)
                {
                    case E.M1:
                        return this.f1;
                    case E.M2:
                        return this.f2;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public E P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum E
    {
        M1,
        M2,
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
        private static readonly int F = 2;

        public int P => F;
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
        private const int F = 2;

        public static int P => F;
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
        private const int F = 2;

        public int P => F;
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
