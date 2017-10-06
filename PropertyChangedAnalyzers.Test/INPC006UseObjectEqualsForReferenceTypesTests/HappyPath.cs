namespace PropertyChangedAnalyzers.Test.INPC006UseObjectEqualsForReferenceTypesTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
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
        public void OneTimeSetUp()
        {
            AnalyzerAssert.SuppressedDiagnostics.Add(INPC006UseReferenceEquals.DiagnosticId);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AnalyzerAssert.ResetMetadataSuppressedDiagnostics();
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

            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
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

            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(FooCode, testCode);
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

            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
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

            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
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

            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
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
            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
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
            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
        }

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
            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
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
            AnalyzerAssert.Valid<INPC006UseObjectEqualsForReferenceTypes>(testCode);
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