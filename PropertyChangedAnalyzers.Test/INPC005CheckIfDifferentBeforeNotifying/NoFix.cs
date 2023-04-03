namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifying;

using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class NoFix
{
    private static readonly SetAccessorAnalyzer Analyzer = new();
    private static readonly CheckIfDifferentBeforeNotifyFix Fix = new();
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
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

    private static readonly IReadOnlyList<TestCaseData> TestCases = new[]
    {
        new TestCaseData("string", "Equals(value, this.p)"),
        new TestCaseData("string", "Equals(this.p, value)"),
        new TestCaseData("string", "Equals(value, p)"),
        new TestCaseData("string", "Equals(value, P)"),
        new TestCaseData("string", "Equals(P, value)"),
        new TestCaseData("string", "Nullable.Equals(value, this.p)"),
        new TestCaseData("int?",   "Nullable.Equals(value, this.p)"),
        new TestCaseData("string", "value.Equals(this.p)"),
        new TestCaseData("string", "value.Equals(p)"),
        new TestCaseData("string", "this.p.Equals(value)"),
        new TestCaseData("string", "p.Equals(value)"),
        new TestCaseData("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.p)"),
        new TestCaseData("string", "ReferenceEquals(value, this.p)"),
    };

    [TestCaseSource(nameof(TestCases))]
    public static void Check(string type, string expression)
    {
        var code = @"
#nullable disable
#pragma warning disable CS8019, CS8616
namespace N
{
    using System;
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
                if (Equals(value, this.p))
                {
                    this.p = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.p)", expression)
.AssertReplace("int", type);

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }

    [TestCaseSource(nameof(TestCases))]
    public static void IfNotEqualsReturnElseAssignAndOnPropertyChanged(string type, string expression)
    {
        var code = @"
#nullable disable
#pragma warning disable CS8019, CS8616
namespace N
{
    using System;
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
                if (!Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("Equals(value, this.p)", expression)
.AssertReplace("int", type);

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }

    [TestCaseSource(nameof(TestCases))]
    public static void IfNotEqualsAssignReturnElseOnPropertyChanged(string type, string expression)
    {
        var code = @"
#nullable disable
#pragma warning disable CS8019, CS8616
namespace N
{
    using System;
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
                if (!Equals(value, this.p))
                {
                    this.p = value;
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
}".AssertReplace("Equals(value, this.p)", expression)
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
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value != this.p)
                {
                    return;
                }

                this.p = value;
                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    this.p = value;
                    return;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    this.p = value;
                }

                ↓this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    this.p = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    this.p = value;
                    ↓this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p2;

        public string P1 => $""Hello {this.P2}"";

        public int P2
        {
            get { return this.p2; }
            set
            {
                if (!this.TrySet(ref this.p2, value))
                {
                    ↓this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, code });
    }

    [Test]
    public static void IfNotTrySetBlockOnPropertyChanged()
    {
        var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged
    {
        private string? p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get => this.p2;
            set
            {
                if (!this.TrySet(ref this.p2, value))
                {
                    ↓this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}";

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, code });
    }

    [Test]
    public static void IfNotTrySetOnPropertyChanged()
    {
        var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged
    {
        private string? p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get => this.p2;
            set
            {
                if (!this.TrySet(ref this.p2, value))
                    ↓this.OnPropertyChanged(nameof(this.P1));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}";

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, code });
    }

    [Test]
    public static void IfTrySetElseOnPropertyChanged()
    {
        var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged
    {
        private string? p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get => this.p2;
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                }
                else
                {
                    ↓this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}";

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, code });
    }
}
