namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class Codefix
    {
        internal class WhenError
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
                new TestCase("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
                new TestCase("string", "ReferenceEquals(value, this.bar)"),
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
                    this.bar = value;
                    ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
                testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call)
                                   .AssertReplace("string", check.Type);
                AnalyzerAssert.NoFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode);
            }

            [TestCaseSource(nameof(TestCases))]
            public void NegatedCheckReturn(TestCase check)
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
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
                testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call)
                                   .AssertReplace("string", check.Type);
                AnalyzerAssert.NoFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode);
            }

            [Test]
            public void OperatorNotEquals()
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
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                AnalyzerAssert.NoFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode);
            }

            [Test]
            public void OperatorNotEqualsReturn()
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
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                AnalyzerAssert.NoFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode);
            }

            [Test]
            public void OperatorEquals()
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
                if (value == this.bar)
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                AnalyzerAssert.NoFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode);
            }

            [Test]
            public void OperatorEqualsNoReturn()
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
                if (value == this.bar)
                {
                    this.bar = value;
                }

                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                AnalyzerAssert.NoFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode);
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
}
