namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    private static readonly MutationAnalyzer Analyzer = new();
    private static readonly NotifyForDependentPropertyFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC003NotifyForDependentProperty);

    [TestCase("this.p = p;")]
    [TestCase("this.p += p;")]
    [TestCase("this.p -= p;")]
    [TestCase("this.p *= p;")]
    [TestCase("this.p /= p;")]
    [TestCase("this.p %= p;")]
    [TestCase("this.p++;")]
    [TestCase("this.p--;")]
    [TestCase("--this.p;")]
    [TestCase("++this.p;")]
    public static void IntFieldUpdatedInMethod(string update)
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public void M(int p)
        {
            ↓this.p = p;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.p = p;", update);

        var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public void M(int p)
        {
            this.p = p;
            this.OnPropertyChanged(nameof(this.P));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.p = p;", update);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("_p = p;")]
    [TestCase("_p += p;")]
    [TestCase("_p -= p;")]
    [TestCase("_p *= p;")]
    [TestCase("_p /= p;")]
    [TestCase("_p %= p;")]
    [TestCase("_p++;")]
    [TestCase("_p--;")]
    [TestCase("--_p;")]
    [TestCase("++_p;")]
    public static void IntFieldUpdatedInMethodUnderscoreNames(string update)
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => _p;

        public void M(int p)
        {
            ↓_p = p;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("_p = p;", update);

        var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => _p;

        public void M(int p)
        {
            _p = p;
            OnPropertyChanged(nameof(P));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("_p = p;", update);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
    }

    [TestCase("this.p |= p;")]
    [TestCase("this.p ^= p;")]
    [TestCase("this.p &= p;")]
    public static void BoolFieldUpdatedInMethod(string update)
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private bool p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool P => this.p;

        public void M(bool p)
        {
            ↓this.p = p;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.p = p;", update);

        var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private bool p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool P => this.p;

        public void M(bool p)
        {
            this.p = p;
            this.OnPropertyChanged(nameof(this.P));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.p = p;", update);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesExpressionBodyStringInterpolation()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesExpressionBodyStringInterpolationInternalClassInternalProperty()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        internal string FullName => $""{this.FirstName} {this.LastName}"";

        internal string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        internal string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        internal string FullName => $""{this.FirstName} {this.LastName}"";

        internal string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        internal string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesCopyLocalNullCheckInvoke()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesExpressionBodyTernarySimple()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string CalculatedName => this.p == null ? ""Missing"" : this.p;

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                ↓this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string CalculatedName => this.p == null ? ""Missing"" : this.p;

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CalculatedName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesExpressionBodyNested()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesExpressionBodyNestedSimple()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("=> CreateP1()")]
    [TestCase("=> this.CreateP1()")]
    public static void WhenUsingPropertiesExpressionCallingMethod(string call)
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => CreateP1();

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        public string CreateP1() => $""Hello {this.FullName}"";

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("=> CreateP1()", call);

        var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => CreateP1();

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        public string CreateP1() => $""Hello {this.FullName}"";

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("=> CreateP1()", call);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesExpressionBodyTernary()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? FullName => this.FirstName == null ? this.LastName : $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? FullName => this.FirstName == null ? this.LastName : $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesStatementBody()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingPropertiesStatementBodyUnderscoreNames()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? _firstName;
        private string? _lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{FirstName}{LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return _firstName;
            }

            set
            {
                if (value == _firstName)
                {
                    return;
                }

                ↓_firstName = value;
                OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return _lastName;
            }

            set
            {
                if (value == _lastName)
                {
                    return;
                }

                _lastName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private string? _firstName;
        private string? _lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{FirstName}{LastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return _firstName;
            }

            set
            {
                if (value == _firstName)
                {
                    return;
                }

                _firstName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return _lastName;
            }

            set
            {
                if (value == _lastName)
                {
                    return;
                }

                _lastName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsExpressionBodyStringInterpolation()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => $""{this.firstName} {this.lastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => $""{this.firstName} {this.lastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsExpressionBodyStringFormat()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => string.Format(""{0} {1}"", this.firstName, this.lastName);

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName => string.Format(""{0} {1}"", this.firstName, this.lastName);

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("this.p")]
    [TestCase("p")]
    public static void WhenUsingBackingFieldExpressionBodyStringToUpper(string path)
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CapsName => this.p?.ToUpper();

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                ↓this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.p", path);

        var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CapsName => this.p?.ToUpper();

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CapsName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.p", path);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingBackingFieldExpressionBodyStringElvisToUpper()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CapsName => this.p?.ToUpper();

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                ↓this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CapsName => this.p?.ToUpper();

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CapsName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsExpressionBodyReturningCreatedObject()
    {
        var personCode = @"
namespace N
{
    public class Person
    {
        public Person(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public string FirstName { get; }

        public string LastName { get; }
    }
}";

        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Person Person => new Person(this.firstName ?? string.Empty, this.lastName ?? string.Empty);

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Person));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Person Person => new Person(this.firstName ?? string.Empty, this.lastName ?? string.Empty);

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Person));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Person));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { personCode, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { personCode, before }, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsExpressionBodyReturningArray()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p2;
        private string? p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string?[] P1 => new[] { this.p2, this.p3 };

        public string? P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                ↓this.p2 = value;
                this.OnPropertyChanged();
            }
        }

        public string? P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? p2;
        private string? p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string?[] P1 => new[] { this.p2, this.p3 };

        public string? P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        public string? P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsYieldReturning()
    {
        var before = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p2;
        private string? p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable<string?> P1
        {
            get
            {
                yield return this.p2;
                yield return this.p3;
            }
        }

        public string? P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                ↓this.p2 = value;
                this.OnPropertyChanged();
            }
        }

        public string? P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        var after = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p2;
        private string? p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable<string?> P1
        {
            get
            {
                yield return this.p2;
                yield return this.p3;
            }
        }

        public string? P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        public string? P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsStatementBody()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.firstName} {this.lastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                ↓this.firstName = value;
                this.OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? firstName;
        private string? lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.firstName} {this.lastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsStatementBodyWithSwitch()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CalculatedName
        {
            get
            {
                switch (this.p)
                {
                    case ""Meh"":
                        return ""heM"";
                    default:
                        return this.p;
                }
            }
        }

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                ↓this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CalculatedName
        {
            get
            {
                switch (this.p)
                {
                    case ""Meh"":
                        return ""heM"";
                    default:
                        return this.p;
                }
            }
        }

        public string? P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CalculatedName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenUsingBackingFieldsStatementBodyUnderscoreNames()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? _firstName;
        private string? _lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{_firstName} {_lastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return _firstName;
            }

            set
            {
                if (value == _firstName)
                {
                    return;
                }

                ↓_firstName = value;
                OnPropertyChanged();
            }
        }

        public string? LastName
        {
            get
            {
                return _lastName;
            }

            set
            {
                if (value == _lastName)
                {
                    return;
                }

                _lastName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private string? _firstName;
        private string? _lastName;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{_firstName} {_lastName}"";
            }
        }

        public string? FirstName
        {
            get
            {
                return _firstName;
            }

            set
            {
                if (value == _firstName)
                {
                    return;
                }

                _firstName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return _lastName;
            }

            set
            {
                if (value == _lastName)
                {
                    return;
                }

                _lastName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after);
    }

    [Test]
    public static void FieldUpdatedInMethodWithInvokerInBaseClass()
    {
        var viewModelBaseCode = @"
namespace N.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        var before = @"
namespace N.Client
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : N.Core.ViewModelBase
    {
        private string? text;

        public string? Text => this.text;

        public void Update(string text)
        {
            ↓this.text = text;
        }
    }
}";

        var after = @"
namespace N.Client
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : N.Core.ViewModelBase
    {
        private string? text;

        public string? Text => this.text;

        public void Update(string text)
        {
            this.text = text;
            this.OnPropertyChanged(nameof(this.Text));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
    }

    [Test]
    public static void InSimpleLambdaExpressionBody()
    {
        var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public C()
        {
            Action<int> action = x => ↓this.p = this.p + ""meh"";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public C()
        {
            Action<int> action = x =>
            {
                this.p = this.p + ""meh"";
                this.OnPropertyChanged(nameof(this.P));
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void InSimpleLambdaStatementBody()
    {
        var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public C()
        {
            Action<int> action = x =>
            {
                ↓this.p = this.p + ""meh"";
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public C()
        {
            Action<int> action = x =>
            {
                this.p = this.p + ""meh"";
                this.OnPropertyChanged(nameof(this.P));
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void InParenthesizedLambdaExpressionBody()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public C()
        {
            this.PropertyChanged += (o, e) => ↓this.p = this.p + ""meh"";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? p;

        public C()
        {
            this.PropertyChanged += (o, e) =>
            {
                this.p = this.p + ""meh"";
                this.OnPropertyChanged(nameof(this.P));
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void InParenthesizedLambdaStatementBody()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string? p;

        public C()
        {
            this.PropertyChanged += (o, e) =>
            {
                ↓this.p = this.p + ""meh"";
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? p;

        public C()
        {
            this.PropertyChanged += (o, e) =>
            {
                this.p = this.p + ""meh"";
                this.OnPropertyChanged(nameof(this.P));
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P => this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void AssigningFieldsInGetter()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p = 1;
        private int getCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int GetCount => this.getCount;

        public int P
        {
            get
            {
                ↓this.getCount++;
                return this.p;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p = 1;
        private int getCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int GetCount => this.getCount;

        public int P
        {
            get
            {
                this.getCount++;
                this.OnPropertyChanged(nameof(this.GetCount));
                return this.p;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("this.c1.P")]
    [TestCase("c1.P")]
    public static void WhenAssigningNestedField(string path)
    {
        var c1 = @"
namespace N
{
    public class C1
    {
        public int P;
    }
}";
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.c1.P;

        public void M(int p)
        {
            ↓this.c1.P = p;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.c1.P", path);
        var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.c1.P;

        public void M(int p)
        {
            this.c1.P = p;
            this.OnPropertyChanged(nameof(this.P));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.c1.P", path);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
    }

    [Test]
    public static void WhenAssigningNestedFieldRoot()
    {
        var c1 = @"
namespace N
{
    public class C1
    {
        public int P;
    }
}";
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private C1 c1 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.c1.P;

        public void Update(int p)
        {
            ↓this.c1 = new C1();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    public class C2 : INotifyPropertyChanged
    {
        private C1 c1 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.c1.P;

        public void Update(int p)
        {
            this.c1 = new C1();
            this.OnPropertyChanged(nameof(this.P));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
    }

    [Test]
    public static void WhenAssigningRootForNestedField()
    {
        var c1 = @"
namespace N
{
    public class C1
    {
        public int F;
    }
}";
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private C1 c1 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.c1.F;

        public void Update()
        {
            ↓this.c1 = new C1();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    public class C2 : INotifyPropertyChanged
    {
        private C1 c1 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.c1.F;

        public void Update()
        {
            this.c1 = new C1();
            this.OnPropertyChanged(nameof(this.P));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after);
    }

    [Test]
    public static void OverriddenProperty()
    {
        var viewModelBase = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public abstract int P { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        var before = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int p;

        public override int P => this.p;

        public void Update(int value)
        {
            ↓this.p = value;
        }
    }
}";
        var after = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int p;

        public override int P => this.p;

        public void Update(int value)
        {
            this.p = value;
            this.OnPropertyChanged(nameof(this.P));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after);
    }

    [Test]
    public static void WhenNotifyingInElseOnly()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public void Update(bool up)
        {
            if (up)
            {
                ↓this.p++;
            }
            else
            {
                this.p--;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    public sealed class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P => this.p;

        public void Update(bool up)
        {
            if (up)
            {
                this.p++;
                this.OnPropertyChanged(nameof(this.P));
            }
            else
            {
                this.p--;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("this.OnPropertyChanged(nameof(this.P2))")]
    public static void WhenDependentExpressionBodiedProperties(string expression)
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 => this.P2 + 1;

        public int P2 => this.P3 * 2;

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.OnPropertyChanged(nameof(this.P2))", expression);
        var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 => this.P2 + 1;

        public int P2 => this.P3 * 2;

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void AddsBeforeExplicitReturn()
    {
        var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 => this.p3 * this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                this.OnPropertyChanged();
                return;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int p3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 => this.p3 * this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                return;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
