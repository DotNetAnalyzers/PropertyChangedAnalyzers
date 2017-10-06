namespace PropertyChangedAnalyzers.Test.INPC006UseObjectEqualsForReferenceTypesTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly IReadOnlyList<TestCase> TestCases = new[]
            {
                new TestCase("object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("Object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(this.bar, value)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(value, bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(value, Bar)", "Equals(value, this.bar)"),
                new TestCase("ReferenceEquals(Bar, value)", "Equals(value, this.bar)"),
                new TestCase("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new TestCase("value.Equals(this.bar)", "Equals(value, this.bar)"),
                new TestCase("value.Equals(bar)", "Equals(value, this.bar)"),
                new TestCase("this.bar.Equals(value)", "Equals(value, this.bar)"),
                new TestCase("bar.Equals(value)", "Equals(value, this.bar)"),
                new TestCase("System.Collections.Generic.EqualityComparer<Foo>.Default.Equals(value, this.bar)", null),
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
            AnalyzerAssert.CodeFix<INPC006UseObjectEqualsForReferenceTypes, UseCorrectEqualityCodeFixProvider>(new[] { FooCode, testCode }, fixedCode);
            AnalyzerAssert.FixAll<INPC006UseObjectEqualsForReferenceTypes, UseCorrectEqualityCodeFixProvider>(new[] { FooCode, testCode }, fixedCode);
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
            AnalyzerAssert.NoFix<INPC006UseObjectEqualsForReferenceTypes, UseCorrectEqualityCodeFixProvider>(FooCode, testCode);
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
}";

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
}";
            testCode = testCode.AssertReplace("ReferenceEquals(value, this.bar)", check.Call);
            fixedCode = check.FixedCall == null
                            ? fixedCode.AssertReplace("Equals(value, this.bar)", check.Call)
                            : fixedCode.AssertReplace("Equals(value, this.bar)", check.FixedCall);
            AnalyzerAssert.CodeFix<INPC006UseObjectEqualsForReferenceTypes, UseCorrectEqualityCodeFixProvider>(new[] { FooCode, testCode }, fixedCode);
            AnalyzerAssert.FixAll<INPC006UseObjectEqualsForReferenceTypes, UseCorrectEqualityCodeFixProvider>(new[] { FooCode, testCode }, fixedCode);
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
}";
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call);
            AnalyzerAssert.NoFix<INPC006UseObjectEqualsForReferenceTypes, UseCorrectEqualityCodeFixProvider>(FooCode, testCode);
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