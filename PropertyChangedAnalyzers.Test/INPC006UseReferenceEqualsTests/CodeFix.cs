namespace PropertyChangedAnalyzers.Test.INPC006UseReferenceEqualsTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly CodeFixProvider Fix = new EqualityFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC006UseReferenceEqualsForReferenceTypes);

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
        {
            new TestCaseData("Equals(value, this.p)",               "ReferenceEquals(value, this.p)"),
            new TestCaseData("Equals(this.p, value)",               "ReferenceEquals(this.p, value)"),
            new TestCaseData("Equals(value, p)",                    "ReferenceEquals(value, p)"),
            new TestCaseData("Equals(value, P)",                    "ReferenceEquals(value, P)"),
            new TestCaseData("Equals(P, value)",                    "ReferenceEquals(P, value)"),
            new TestCaseData("Equals(value, this.P)",               "ReferenceEquals(value, this.P)"),
            new TestCaseData("string.Equals(value, this.p)",        "ReferenceEquals(value, this.p)"),
            new TestCaseData("String.Equals(value, this.P)",        "ReferenceEquals(value, this.P)"),
            new TestCaseData("System.String.Equals(value, this.p)", "ReferenceEquals(value, this.p)"),
            new TestCaseData("Nullable.Equals(value, this.p)",      "ReferenceEquals(value, this.p)"),
            new TestCaseData("Nullable.Equals(value, this.p)",      "ReferenceEquals(value, this.p)"),
            new TestCaseData("value.Equals(this.p)",                "ReferenceEquals(value, this.p)"),
            new TestCaseData("value.Equals(p)",                     "ReferenceEquals(value, p)"),
            new TestCaseData("this.p.Equals(value)",                "ReferenceEquals(this.p, value)"),
            new TestCaseData("p.Equals(value)",                     "ReferenceEquals(p, value)"),
        };

        private const string ReferenceType = @"
namespace N
{
    public class ReferenceType
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

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
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
}".AssertReplace("Equals(value, this.p)", expressionBefore);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
        {
            get { return this.p; }
            set
            {
                if (Equals(value, this.p))
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
}".AssertReplace("Equals(value, this.p)", expressionAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
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

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
        {
            get { return this.p; }
            set
            {
                if (!↓Equals(value, this.p))
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
}".AssertReplace("Equals(value, this.p)", expressionBefore);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
        {
            get { return this.p; }
            set
            {
                if (!Equals(value, this.p))
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
}".AssertReplace("Equals(value, this.p)", expressionAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
        }

        [Test]
        public static void ConstrainedGeneric()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C<T> : INotifyPropertyChanged
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

    public class C<T> : INotifyPropertyChanged
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
        }

        [Test]
        public static void OperatorEquals()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
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

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
        }

        [Test]
        public static void OperatorEqualsInternalClassInternalProperty()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal ReferenceType P
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

    internal class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal ReferenceType P
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
        }

        [Test]
        public static void OperatorNotEquals()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
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

    public class C : INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
        }
    }
}
