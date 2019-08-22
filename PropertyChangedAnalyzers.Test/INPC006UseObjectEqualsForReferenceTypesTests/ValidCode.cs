namespace PropertyChangedAnalyzers.Test.INPC006UseObjectEqualsForReferenceTypesTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new EqualityAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC006UseObjectEqualsForReferenceTypes;

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
        {
            new TestCaseData("Foo", "Equals(value, this.bar)"),
            new TestCaseData("Foo", "Equals(this.bar, value)"),
            new TestCaseData("Foo", "Equals(value, bar)"),
            new TestCaseData("Foo", "Equals(value, Bar)"),
            new TestCaseData("Foo", "Equals(Bar, value)"),
            new TestCaseData("Foo", "Nullable.Equals(value, this.bar)"),
            new TestCaseData("Foo", "value.Equals(this.bar)"),
            new TestCaseData("Foo", "value.Equals(bar)"),
            new TestCaseData("Foo", "this.bar.Equals(value)"),
            new TestCaseData("Foo", "bar.Equals(value)"),
            new TestCaseData("int?",   "Nullable.Equals(value, this.bar)"),
            new TestCaseData("string", "value.Equals(this.bar)"),
            new TestCaseData("string", "value.Equals(bar)"),
            new TestCaseData("string", "this.bar.Equals(value)"),
            new TestCaseData("string", "bar.Equals(value)"),
            new TestCaseData("string", "string.Equals(value, this.bar, StringComparison.OrdinalIgnoreCase)"),
            new TestCaseData("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
        };

        private static readonly string Foo = @"
namespace RoslynSandbox
{
    public class Foo
    {
    }
}";

        [TestCaseSource(nameof(TestCases))]
        public static void Check(string type, string expression)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get => this.bar;
            set
            {
                if (Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("Foo", type);

            RoslynAssert.Valid(Analyzer, Descriptor, Foo, code);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void CheckNegated(string type, string expression)
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get => this.bar;
            set
            {
                if (!Equals(value, this.bar))
                {
                    this.bar = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("Foo", type);

            RoslynAssert.Valid(Analyzer, Descriptor, Foo, code);
        }

        [Test]
        public static void SimpleProperty()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set { this.bar = value; }
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
        public static void CallsRaisePropertyChangedWithEventArgsIfReturn()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar)) return;
                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, Foo, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfReturnUseProperty()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.Bar) return;
                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfBody()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value != this.bar)
                {
                    this.bar = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedCallerMemberName()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged();
            }
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
        public static void Invokes()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void InvokesCached()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs BarPropertyChangedArgs = new PropertyChangedEventArgs(nameof(Bar));
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, BarPropertyChangedArgs);
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void IgnoreEqualityComparerEquals()
        {
            var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo foo;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Foo
        {
            get { return this.foo; }
            set
            {
                if (System.Collections.Generic.EqualityComparer<Foo>.Default.Equals(value, this.foo))
                {
                    return;
                }

                this.foo = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, fooCode, code);
        }

        [Test]
        public static void IgnoreNegatedEqualityComparerEquals()
        {
            var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
    }
}";

            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo foo;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Foo
        {
            get { return this.foo; }
            set
            {
                if (!System.Collections.Generic.EqualityComparer<Foo>.Default.Equals(value, this.foo))
                {
                    this.foo = value;
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
            RoslynAssert.Valid(Analyzer, Descriptor, fooCode, code);
        }

        [TestCase("string")]
        public static void OperatorEquals(string type)
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
            get => this.p;
            set
            {
                if (value == this.p)
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
}".AssertReplace("string", type);

            RoslynAssert.Valid(Analyzer, Descriptor,  code);
        }

        [TestCase("string")]
        public static void OperatorNotEquals(string type)
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
}".AssertReplace("string", type);

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
