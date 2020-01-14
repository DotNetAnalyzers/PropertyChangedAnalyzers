namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class NoFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly CodeFixProvider Fix = new CheckIfDifferentBeforeNotifyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC005CheckIfDifferentBeforeNotifying);

        private const string ViewModelBaseCode = @"
namespace N.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
        {
                new TestCaseData("string", "Equals(value, this.bar)"),
                new TestCaseData("string", "Equals(this.bar, value)"),
                new TestCaseData("string", "Equals(value, bar)"),
                new TestCaseData("string", "Equals(value, Bar)"),
                new TestCaseData("string", "Equals(Bar, value)"),
                new TestCaseData("string", "Nullable.Equals(value, this.bar)"),
                new TestCaseData("int?",   "Nullable.Equals(value, this.bar)"),
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
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
.AssertReplace("int", type);

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void IfNotEqualsReturnElseAssignAndOnPropertyChanged(string type, string expression)
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
.AssertReplace("int", type);

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void IfNotEqualsAssignReturnElseOnPropertyChanged(string type, string expression)
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
                {
                    this.bar = value;
                    return;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.bar)", expression)
.AssertReplace("int", type);

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [Test]
        public static void IfOperatorNotEqualsReturn()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [Ignore("#87")]
        [Test]
        public static void OperatorEqualsNoAssignReturn()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
                    return;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [Ignore("Don't think this is the correct warning here.")]
        [Test]
        public static void IfOperatorEqualsAssignThenOnPropertyChanged()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    this.bar = value;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [Test]
        public static void IfOperatorEqualsAssignAndNotify()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [Test]
        public static void OperatorEquals()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [Test]
        public static void InsideIfNegatedTrySet()
        {
            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (!this.TrySet(ref this.name, value))
                {
                    ↓this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, code });
        }
    }
}
