namespace PropertyChangedAnalyzers.Test.INPC006UseReferenceEqualsTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new IfStatementAnalyzer();

        private static readonly IReadOnlyList<TestCase> TestCases = new[]
        {
            new TestCase("string", "Equals(value, this.bar)"),
            new TestCase("string", "Equals(this.bar, value)"),
            new TestCase("string", "Equals(value, bar)"),
            new TestCase("string", "Equals(value, Bar)"),
            new TestCase("string", "Equals(Bar, value)"),
            new TestCase("string", "Nullable.Equals(value, this.bar)"),
            new TestCase("int?", "Nullable.Equals(value, this.bar)"),
            new TestCase("string", "value.Equals(this.bar)"),
            new TestCase("string", "value.Equals(bar)"),
            new TestCase("string", "this.bar.Equals(value)"),
            new TestCase("string", "bar.Equals(value)"),
            new TestCase("string", "string.Equals(value, this.bar, StringComparison.OrdinalIgnoreCase)"),
            new TestCase("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
            new TestCase("string", "ReferenceEquals(value, this.bar)"),
        };

        private static readonly string FooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
    }
}";

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            RoslynAssert.SuppressedDiagnostics.Add(INPC006UseObjectEqualsForReferenceTypes.DiagnosticId);
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            RoslynAssert.ResetAll();
        }

        [Test]
        public static void SimpleProperty()
        {
            var testCode = @"
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

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfReturn()
        {
            var testCode = @"
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
                if (ReferenceEquals(value, this.bar)) return;
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
            RoslynAssert.Valid(Analyzer, FooCode, testCode);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfReturnUseProperty()
        {
            var testCode = @"
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

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfBody()
        {
            var testCode = @"
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

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void CallsRaisePropertyChangedCallerMemberName()
        {
            var testCode = @"
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

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void Invokes()
        {
            var testCode = @"
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void InvokesCached()
        {
            var testCode = @"
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void Check(TestCase check)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("Equals(value, this.bar)", check.Call).AssertReplace("string", check.Type);

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void NegatedCheck(TestCase check)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
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
}".AssertReplace("Equals(value, this.bar)", check.Call)
  .AssertReplace("string", check.Type);

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void IgnoreGeneric()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel<T> : INotifyPropertyChanged
    {
        private T bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public T Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        public class TestCase
        {
            public TestCase(string type, string call)
            {
                this.Type = type;
                this.Call = call;
            }

            internal string Type { get; }

            internal string Call { get; }

            public override string ToString()
            {
                return $"{nameof(this.Type)}: {this.Type}, {nameof(this.Call)}: {this.Call}";
            }
        }
    }
}
