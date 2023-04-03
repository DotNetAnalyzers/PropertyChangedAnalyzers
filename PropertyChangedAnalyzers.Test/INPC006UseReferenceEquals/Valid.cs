namespace PropertyChangedAnalyzers.Test.INPC006UseReferenceEquals;

using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static class Valid
{
    private static readonly SetAccessorAnalyzer Analyzer = new();

    private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC006UseReferenceEqualsForReferenceTypes;

    private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
    {
        new TestCaseData("ReferenceType", "ReferenceEquals(value, this.p)"),
        new TestCaseData("ReferenceType", "ReferenceEquals(this.p, value)"),
        new TestCaseData("ReferenceType", "ReferenceEquals(value, p)"),
        new TestCaseData("ReferenceType", "ReferenceEquals(value, P)"),
        new TestCaseData("ReferenceType", "ReferenceEquals(P, value)"),
        new TestCaseData("int?",          "Nullable.Equals(value, this.p)"),
        new TestCaseData("string",        "value.Equals(this.p)"),
        new TestCaseData("string",        "value.Equals(p)"),
        new TestCaseData("string",        "this.p.Equals(value)"),
        new TestCaseData("string",        "p.Equals(value)"),
        new TestCaseData("string",        "string.Equals(value, this.p, StringComparison.OrdinalIgnoreCase)"),
        new TestCaseData("string",        "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.p)"),
        new TestCaseData("string",        "ReferenceEquals(value, this.p)"),
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
#pragma warning disable CS8019
#nullable disable
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
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("Equals(value, this.p)", expression)
.AssertReplace("ReferenceType", type);

        RoslynAssert.Valid(Analyzer, Descriptor, ReferenceType, code);
    }

    [TestCaseSource(nameof(TestCases))]
    public static void CheckNegated(string type, string expression)
    {
        var code = @"
#pragma warning disable CS8019
#nullable disable
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
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("Equals(value, this.p)", expression)
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

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

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

    public class C : INotifyPropertyChanged
    {
        private ReferenceType? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReferenceType? P
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

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

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

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        public event PropertyChangedEventHandler? PropertyChanged;

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
        public event PropertyChangedEventHandler? PropertyChanged;

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
        private T? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public T? P
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
