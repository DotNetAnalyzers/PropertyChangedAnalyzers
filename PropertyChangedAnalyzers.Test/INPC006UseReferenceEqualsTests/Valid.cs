namespace PropertyChangedAnalyzers.Test.INPC006UseReferenceEqualsTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new EqualityAnalyzer();

        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC006UseReferenceEqualsForReferenceTypes;

        private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
        {
            new TestCaseData("ReferenceType",    "ReferenceEquals(value, this.bar)"),
            new TestCaseData("ReferenceType",    "ReferenceEquals(this.bar, value)"),
            new TestCaseData("ReferenceType",    "ReferenceEquals(value, bar)"),
            new TestCaseData("ReferenceType",    "ReferenceEquals(value, Bar)"),
            new TestCaseData("ReferenceType",    "ReferenceEquals(Bar, value)"),
            new TestCaseData("int?",             "Nullable.Equals(value, this.bar)"),
            new TestCaseData("string",           "value.Equals(this.bar)"),
            new TestCaseData("string",           "value.Equals(bar)"),
            new TestCaseData("string",           "this.bar.Equals(value)"),
            new TestCaseData("string",           "bar.Equals(value)"),
            new TestCaseData("string",           "string.Equals(value, this.bar, StringComparison.OrdinalIgnoreCase)"),
            new TestCaseData("string",           "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
            new TestCaseData("string",           "ReferenceEquals(value, this.bar)"),
        };

        private const string ReferenceType = @"
namespace N
{
    public class ReferenceType
    {
    }
}";

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
        private ReferenceType bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType Bar
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
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("ReferenceType", type);

            RoslynAssert.Valid(Analyzer, Descriptor, ReferenceType, code);
        }

        [TestCaseSource(nameof(TestCases))]
        public static void CheckNegated(string type, string expression)
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private ReferenceType bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
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
}".AssertReplace("Equals(value, this.bar)", expression)
  .AssertReplace("ReferenceType", type);

            RoslynAssert.Valid(Analyzer, Descriptor, ReferenceType, code);
        }

        [Test]
        public static void SimpleProperty()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set { this.p = value; }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfReturn()
        {
            var code = @"
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
                if (ReferenceEquals(value, this.p)) return;
                this.p = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, ReferenceType, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfReturnUseProperty()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.P) return;
                this.p = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedWithEventArgsIfBody()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value != this.p)
                {
                    this.p = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedCallerMemberName()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void Invokes()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.P)));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void InvokesCached()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PPropertyChangedArgs = new PropertyChangedEventArgs(nameof(P));
        private int p;
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.PropertyChanged?.Invoke(this, PPropertyChangedArgs);
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void IgnoreGeneric()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C<T> : INotifyPropertyChanged
    {
        private T p;
        public event PropertyChangedEventHandler PropertyChanged;

        public T P
        {
            get { return this.p; }
            set
            {
                if (Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.P)));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
