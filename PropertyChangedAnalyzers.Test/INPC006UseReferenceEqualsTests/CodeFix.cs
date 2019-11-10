namespace PropertyChangedAnalyzers.Test.INPC006UseReferenceEqualsTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new EqualityAnalyzer();
        private static readonly CodeFixProvider Fix = new EqualityFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC006UseReferenceEqualsForReferenceTypes);

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
            {
                new TestCaseData("Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCaseData("Equals(this.bar, value)", "ReferenceEquals(this.bar, value)"),
                new TestCaseData("Equals(value, bar)", "ReferenceEquals(value, bar)"),
                new TestCaseData("Equals(value, Bar)", "ReferenceEquals(value, Bar)"),
                new TestCaseData("Equals(Bar, value)", "ReferenceEquals(Bar, value)"),
                new TestCaseData("Equals(value, this.Bar)", "ReferenceEquals(value, this.Bar)"),
                new TestCaseData("string.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCaseData("String.Equals(value, this.Bar)", "ReferenceEquals(value, this.Bar)"),
                new TestCaseData("System.String.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCaseData("Nullable.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCaseData("Nullable.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCaseData("value.Equals(this.bar)", "ReferenceEquals(value, this.bar)"),
                new TestCaseData("value.Equals(bar)", "ReferenceEquals(value, bar)"),
                new TestCaseData("this.bar.Equals(value)", "ReferenceEquals(this.bar, value)"),
                new TestCaseData("bar.Equals(value)", "ReferenceEquals(bar, value)"),
                //new TestCaseData("System.Collections.Generic.EqualityComparer<C1>.Default.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
            };

        private const string C1Code = @"
namespace N
{
    public class C1
    {
    }
}";

        [TestCaseSource(nameof(TestCases))]
        public static void Check(string expressionBefore, string expressionAfter)
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 Bar
        {
            get { return this.bar; }
            set
            {
                if (↓Equals(value, this.bar))
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
}".AssertReplace("Equals(value, this.bar)", expressionBefore);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 Bar
        {
            get { return this.bar; }
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
}".AssertReplace("Equals(value, this.bar)", expressionAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void CheckNegated(string expressionBefore, string expressionAfter)
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 Bar
        {
            get { return this.bar; }
            set
            {
                if (!↓Equals(value, this.bar))
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
}".AssertReplace("Equals(value, this.bar)", expressionBefore);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 Bar
        {
            get { return this.bar; }
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
}".AssertReplace("Equals(value, this.bar)", expressionAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
        }

        [Test]
        public static void ConstrainedGeneric()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel<T> : INotifyPropertyChanged
        where T : class
    {
        private T p;
        public event PropertyChangedEventHandler PropertyChanged;

        public T P
        {
            get { return this.p; }
            set
            {
                if (↓Equals(value, this.p))
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
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel<T> : INotifyPropertyChanged
        where T : class
    {
        private T p;
        public event PropertyChangedEventHandler PropertyChanged;

        public T P
        {
            get { return this.p; }
            set
            {
                if (ReferenceEquals(value, this.p))
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
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
        }

        [Test]
        public static void OperatorEquals()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 P
        {
            get { return this.p; }
            set
            {
                if (↓value == this.p)
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
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 P
        {
            get { return this.p; }
            set
            {
                if (ReferenceEquals(value, this.p))
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
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
        }

        [Test]
        public static void OperatorEqualsInternalClassInternalProperty()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private C1 p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal C1 P
        {
            get { return this.p; }
            set
            {
                if (↓value == this.p)
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
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private C1 p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal C1 P
        {
            get { return this.p; }
            set
            {
                if (ReferenceEquals(value, this.p))
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
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
        }

        [Test]
        public static void OperatorNotEquals()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 P
        {
            get { return this.p; }
            set
            {
                if (↓value != this.p)
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
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private C1 p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C1 P
        {
            get { return this.p; }
            set
            {
                if (!ReferenceEquals(value, this.p))
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
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { C1Code, before }, after);
        }
    }
}
