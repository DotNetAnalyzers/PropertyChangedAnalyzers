namespace PropertyChangedAnalyzers.Test.INPC006UseObjectEqualsForReferenceTypesTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        public static readonly IReadOnlyList<EqualsItem> EqualsSource = new[]
            {
                new EqualsItem("object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("Object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(this.bar, value)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(value, bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(value, Bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(Bar, value)", "Equals(value, this.bar)"),
                new EqualsItem("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("value.Equals(this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("value.Equals(bar)", "Equals(value, this.bar)"),
                new EqualsItem("this.bar.Equals(value)", "Equals(value, this.bar)"),
                new EqualsItem("bar.Equals(value)", "Equals(value, this.bar)"),
                new EqualsItem("System.Collections.Generic.EqualityComparer<Foo>.Default.Equals(value, this.bar)", null),
            };

        private static readonly string FooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
    }
}";

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

        [TestCaseSource(nameof(EqualsSource))]
        public void Check(EqualsItem check)
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

        [TestCaseSource(nameof(EqualsSource))]
        public void NegatedCheck(EqualsItem check)
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

        public class EqualsItem
        {
            public EqualsItem(string call, string fixedCall)
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