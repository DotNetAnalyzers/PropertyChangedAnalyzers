namespace PropertyChangedAnalyzers.Test.INPC006UseObjectEqualsForReferenceTypesTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new IfStatementAnalyzer();
        private static readonly CodeFixProvider Fix = new UseCorrectEqualityFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC006_b");

        private static readonly IReadOnlyList<TestCase> TestCases = new[]
            {
                new TestCase("object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("Object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(this.bar, value)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(value, bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(value, Bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(Bar, value)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(value, this.Bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(this.Bar, value)", "Equals(value, this.bar)"),
                new TestCase("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("value.Equals(this.bar)", "Equals(value, this.bar)"),
                new TestCase("value.Equals(bar)", "Equals(value, this.bar)"),
                new TestCase("this.bar.Equals(value)", "Equals(value, this.bar)"),
                new TestCase("bar.Equals(value)", "Equals(value, this.bar)"),
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
            RoslynAssert.SuppressedDiagnostics.Add(INPC006UseReferenceEqualsForReferenceTypes.DiagnosticId);
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            RoslynAssert.ResetAll();
        }

        [TestCaseSource(nameof(TestCases))]
        public static void Check(TestCase check)
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
                ↓if (ReferenceEquals(value, this.bar))
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
}".AssertReplace("ReferenceEquals(value, this.bar)", check.Call);

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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
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

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, FooCode, testCode);
        }

        [Test]
        public static void OperatorEquals()
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
        }

        [Test]
        public static void OperatorEqualsInternalClassInternalProperty()
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode }, fixedCode);
        }

        [Test]
        public static void OperatorNotEquals()
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
            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, new[] { FooCode, testCode });
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
