namespace PropertyChangedAnalyzers.Test.INPC006UseObjectEqualsForReferenceTypesTests
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
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC006UseObjectEqualsForReferenceTypes);

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
            {
                new TestCaseData("object.ReferenceEquals(value, this.p)", "Equals(value, this.p)"),
                new TestCaseData("Object.ReferenceEquals(value, this.p)", "Equals(value, this.p)"),
                new TestCaseData("System.Object.ReferenceEquals(value, this.p)", "Equals(value, this.p)"),
                new TestCaseData("ReferenceEquals(value, this.p)", "Equals(value, this.p)"),
                new TestCaseData("ReferenceEquals(this.p, value)", "Equals(this.p, value)"),
                new TestCaseData("ReferenceEquals(value, p)", "Equals(value, p)"),
                new TestCaseData("ReferenceEquals(value, P)", "Equals(value, P)"),
                new TestCaseData("ReferenceEquals(value, this.P)", "Equals(value, this.P)"),
                new TestCaseData("ReferenceEquals(value, this.p)", "Equals(value, this.p)"),
                //new TestCaseData("Nullable.Equals(value, this.p)", "Equals(value, this.p)"),
                //new TestCaseData("Nullable.Equals(value, this.p)", "Equals(value, this.p)"),
                //new TestCaseData("string.Equals(value, this.p)", "Equals(value, this.p)"),
                //new TestCaseData("String.Equals(value, this.p)", "Equals(value, this.p)"),
                //new TestCaseData("System.String.Equals(value, this.p)", "Equals(value, this.p)"),
                //new TestCaseData("value.Equals(this.p)", "Equals(value, this.p)"),
                //new TestCaseData("value.Equals(p)", "Equals(value, this.p)"),
                //new TestCaseData("this.p.Equals(value)", "Equals(value, this.p)"),
                //new TestCaseData("p.Equals(value)", "Equals(value, this.p)"),
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

    public class C: INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
        {
            get { return this.p; }
            set
            {
                if (↓ReferenceEquals(value, this.p))
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
}".AssertReplace("ReferenceEquals(value, this.p)", expressionBefore);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C: INotifyPropertyChanged
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

    public class C: INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
        {
            get { return this.p; }
            set
            {
                if (!↓ReferenceEquals(value, this.p))
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
}".AssertReplace("ReferenceEquals(value, this.p)", expressionBefore);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C: INotifyPropertyChanged
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
        public static void OperatorEquals()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C: INotifyPropertyChanged
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

    public class C: INotifyPropertyChanged
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

    internal class C: INotifyPropertyChanged
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

    internal class C: INotifyPropertyChanged
    {
        private ReferenceType p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal ReferenceType P
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

    public class C: INotifyPropertyChanged
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

    public class C: INotifyPropertyChanged
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
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
        }
    }
}
