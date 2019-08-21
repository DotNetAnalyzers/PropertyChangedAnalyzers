namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class WhenError
        {
            private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
            {
                new TestCaseData("string", "Equals(value, this.bar)"), 
                new TestCaseData("string", "Equals(this.bar, value)"),
                new TestCaseData("string", "Equals(value, bar)"),
                new TestCaseData("string", "Equals(value, Bar)"),
                new TestCaseData("string", "Equals(Bar, value)"),
                new TestCaseData("string", "Nullable.Equals(value, this.bar)"),
                new TestCaseData("int?", "Nullable.Equals(value, this.bar)"),
                new TestCaseData("string", "value.Equals(this.bar)"),
                new TestCaseData("string", "value.Equals(bar)"),
                new TestCaseData("string", "this.bar.Equals(value)"),
                new TestCaseData("string", "bar.Equals(value)"),
                new TestCaseData("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
                new TestCaseData("string", "ReferenceEquals(value, this.bar)"),
            };

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
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("string", type);

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
            }

            [TestCaseSource(nameof(TestCases))]
            public static void NegatedCheckReturn(string type, string expression)
            {
                var code = @"
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
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("string", type);

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
            }

            [Test]
            public static void OperatorNotEquals()
            {
                var before = @"
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

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }

            [Test]
            public static void OperatorNotEqualsReturn()
            {
                var before = @"
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

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }

            [Test]
            public static void OperatorEquals()
            {
                var before = @"
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

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }

            [Test]
            public static void OperatorEqualsNoReturn()
            {
                var before = @"
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

                RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
            }
        }
    }
}
