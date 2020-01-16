namespace PropertyChangedAnalyzers.Test.INPC022EqualToBackingFieldTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly CodeFixProvider Fix = new ReplaceExpressionFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC022EqualToBackingField);

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
        {
            new TestCaseData("object.ReferenceEquals(value, ↓this.f)",        "object.ReferenceEquals(value, this.p)"),
            new TestCaseData("Object.ReferenceEquals(value, ↓this.f)",        "Object.ReferenceEquals(value, this.p)"),
            new TestCaseData("System.Object.ReferenceEquals(value, ↓this.f)", "System.Object.ReferenceEquals(value, this.p)"),
            new TestCaseData("ReferenceEquals(value, ↓this.f)",               "ReferenceEquals(value, this.p)"),
            new TestCaseData("ReferenceEquals(↓this.f, value)",               "ReferenceEquals(this.p, value)"),
            new TestCaseData("ReferenceEquals(↓f, value)",                    "ReferenceEquals(this.p, value)"),
            new TestCaseData("ReferenceEquals(value, ↓f)",                    "ReferenceEquals(value, this.p)"),
            new TestCaseData("value == ↓f",                                   "value == this.p"),
        };

        private const string ReferenceType = @"
namespace N
{
    public class ReferenceType
    {
    }
}";

        [Test]
        public static void Message()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C: INotifyPropertyChanged
    {
        private int p;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (value == ↓this.f)
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
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C: INotifyPropertyChanged
    {
        private int p;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage("Comparison should be with backing field."), before, after, fixTitle: "Use: this.p");
        }

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
        private ReferenceType f;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
        {
            get => this.p;
            set
            {
                if (ReferenceEquals(value, ↓this.f))
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
}".AssertReplace("ReferenceEquals(value, ↓this.f)", expressionBefore);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C: INotifyPropertyChanged
    {
        private ReferenceType p;
        private ReferenceType f;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType P
        {
            get => this.p;
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
}".AssertReplace("ReferenceEquals(value, this.p)", expressionAfter);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ReferenceType, before }, after);
        }
    }
}
