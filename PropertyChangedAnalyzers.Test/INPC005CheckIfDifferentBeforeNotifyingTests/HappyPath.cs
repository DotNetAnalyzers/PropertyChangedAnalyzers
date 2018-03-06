namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        private static readonly InvocationAnalyzer Analyzer = new InvocationAnalyzer();

        private static readonly IReadOnlyList<TestCase> TestCases = new[]
        {
            new TestCase("string", "Equals(value, this.bar)"),
            new TestCase("string", "Equals(this.bar, value)"),
            new TestCase("string", "Equals(value, bar)"),
            new TestCase("string", "Equals(value, Bar)"),
            new TestCase("string", "Equals(Bar, value)"),
            new TestCase("string", "object.Equals(Bar, value)"),
            new TestCase("string", "Object.Equals(Bar, value)"),
            new TestCase("string", "System.Object.Equals(Bar, value)"),
            new TestCase("string", "Nullable.Equals(value, this.bar)"),
            new TestCase("int?", "Nullable.Equals(value, this.bar)"),
            new TestCase("int?", "System.Nullable.Equals(value, this.bar)"),
            new TestCase("string", "value.Equals(this.bar)"),
            new TestCase("string", "value.Equals(bar)"),
            new TestCase("string", "this.bar.Equals(value)"),
            new TestCase("string", "bar.Equals(value)"),
            new TestCase("string", "string.Equals(value, this.bar, StringComparison.OrdinalIgnoreCase)"),
            new TestCase("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
            new TestCase("string", "ReferenceEquals(value, this.bar)"),
            new TestCase("string", "object.ReferenceEquals(value, this.bar)"),
            new TestCase("string", "Object.ReferenceEquals(value, this.bar)"),
            new TestCase("string", "System.Object.ReferenceEquals(value, this.bar)"),
        };

        [TestCaseSource(nameof(TestCases))]
        public void Check(TestCase check)
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
}";
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call).AssertReplace("string", check.Type);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCaseSource(nameof(TestCases))]
        public void NegatedCheck(TestCase check)
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
}";
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call).AssertReplace("string", check.Type);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void SimpleProperty()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsRaisePropertyChangedWithEventArgsIfReturn()
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
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsRaisePropertyChangedWithEventArgsIfReturnUseProperty()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsRaisePropertyChangedWithEventArgsIfBody()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsRaisePropertyChangedCallerMemberName()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void Invokes()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void InvokesCached()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CheckSideEffectReturn()
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
        private int misses;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    misses++;
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
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WithCheckAndThrowBefore()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class ViewModel : System.ComponentModel.INotifyPropertyChanged
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("if (Math.Abs(value - this.value) < 1e-6)")]
        [TestCase("if (Math.Abs(this.value - value) < 1e-6)")]
        public void WithCheckAndThrowBefore(string code)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class ViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private double value;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public double Value
        {
            get => value;

            set
            {
                if (Math.Abs(value - this.value) < 1e-6)
                {
                    return;
                }

                this.value = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            testCode = testCode.AssertReplace("if (Math.Abs(value - this.value) < 1e-6)", code);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CheckInLock()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly object _busyLock = new object();
        private bool _value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool Value
        {
            get => _value;
            private set
            {
                lock (_busyLock)
                {
                    if (value && _value)
                    {
                        throw new InvalidOperationException();
                    }

                    if (value == _value)
                    {
                        return;
                    }

                    _value = value;
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CheckLockCheck()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly object _gate = new object();
        private object _value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public object Value
        {
            get => _value;
            private set
            {
                if (ReferenceEquals(value, _value))
                {
                    return;
                }

                lock (_gate)
                {
                    if (ReferenceEquals(value, _value))
                    {
                        return;
                    }

                    _value = value;
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
            AnalyzerAssert.Valid(Analyzer, testCode);
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
