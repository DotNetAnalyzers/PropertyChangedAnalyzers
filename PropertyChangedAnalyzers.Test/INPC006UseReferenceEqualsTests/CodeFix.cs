namespace PropertyChangedAnalyzers.Test.INPC006UseReferenceEqualsTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new IfStatementAnalyzer();
        private static readonly CodeFixProvider Fix = new UseCorrectEqualityFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC006_a");

        private static readonly IReadOnlyList<TestCase> TestCases = new[]
            {
                new TestCase("Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Equals(this.bar, value)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Equals(value, bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Equals(value, Bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Equals(Bar, value)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Equals(value, this.Bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Equals(this.Bar, value)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Nullable.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("Nullable.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("value.Equals(this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("value.Equals(bar)", "ReferenceEquals(value, this.bar)"),
                new TestCase("this.bar.Equals(value)", "ReferenceEquals(value, this.bar)"),
                new TestCase("bar.Equals(value)", "ReferenceEquals(value, this.bar)"),
                new TestCase("System.Collections.Generic.EqualityComparer<Foo>.Default.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
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
            AnalyzerAssert.SuppressedDiagnostics.Add(INPC006UseObjectEqualsForReferenceTypes.DiagnosticId);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AnalyzerAssert.ResetAll();
        }

        [Test]
        public void ConstrainedGeneric()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel<T> : INotifyPropertyChanged
        where T : class
    {
        private T bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public T Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
            }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel<T> : INotifyPropertyChanged
        where T : class
    {
        private T bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public T Bar
        {
            get { return this.bar; }
            set
            {
                if (ReferenceEquals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
            }
        }
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
            AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
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
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (value == this.bar)
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

            var fixedCode = @"
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
                if (ReferenceEquals(value, this.bar))
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
            AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
        }

        [Test]
        public void OperatorEqualsInternalClassInternalProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        internal Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (value == this.bar)
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

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        internal Foo Bar
        {
            get { return this.bar; }
            set
            {
                if (ReferenceEquals(value, this.bar))
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
            AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
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
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (value != this.bar)
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

            AnalyzerAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, FooCode, testCode);
        }

        [TestCaseSource(nameof(TestCases))]
        public void Check(TestCase check)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (Equals(value, this.bar))
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
}".AssertReplace("Equals(value, this.bar)", check.Call);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
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
}".AssertReplace("Equals(value, this.bar)", check.FixedCall ?? check.Call);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
            AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
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
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (!Equals(value, this.bar))
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
}".AssertReplace("Equals(value, this.bar)", check.Call);

            AnalyzerAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, FooCode, testCode);
        }

        public class TestCase
        {
            public TestCase(string call, string fixedCall)
            {
                this.Call = call;
                this.FixedCall = fixedCall;
            }

            internal string Call { get; }

            internal string FixedCall { get; }

            public override string ToString() => this.Call;
        }
    }
}
