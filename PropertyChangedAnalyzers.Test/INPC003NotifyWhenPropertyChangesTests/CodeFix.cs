namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new INPC003NotifyWhenPropertyChanges();
        private static readonly CodeFixProvider Fix = new NotifyPropertyChangedFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC003");

        [TestCase("this.value = value;")]
        [TestCase("this.value += value;")]
        [TestCase("this.value -= value;")]
        [TestCase("this.value *= value;")]
        [TestCase("this.value /= value;")]
        [TestCase("this.value %= value;")]
        [TestCase("this.value++;")]
        [TestCase("this.value--;")]
        [TestCase("--this.value;")]
        [TestCase("++this.value;")]
        public static void IntFieldUpdatedInMethod(string update)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        public void Update(int value)
        {
            ↓this.value = value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.value = value;", update);

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        public void Update(int value)
        {
            this.value = value;
            this.OnPropertyChanged(nameof(this.Value));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.value = value;", update);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("_value = value;")]
        [TestCase("_value += value;")]
        [TestCase("_value -= value;")]
        [TestCase("_value *= value;")]
        [TestCase("_value /= value;")]
        [TestCase("_value %= value;")]
        [TestCase("_value++;")]
        [TestCase("_value--;")]
        [TestCase("--_value;")]
        [TestCase("++_value;")]
        public static void IntFieldUpdatedInMethodUnderscoreNames(string update)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly int Meh = 2;
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => _value;

        public void Update(int value)
        {
            ↓_value = value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("_value = value;", update);

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly int Meh = 2;
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => _value;

        public void Update(int value)
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("_value = value;", update);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("this.value |= value;")]
        [TestCase("this.value ^= value;")]
        [TestCase("this.value &= value;")]
        public static void BoolFieldUpdatedInMethod(string update)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool value;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Value => this.value;

        public void Update(bool value)
        {
            ↓this.value = value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.value = value;", update);

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool value;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Value => this.value;

        public void Update(bool value)
        {
            this.value = value;
            this.OnPropertyChanged(nameof(this.Value));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.value = value;", update);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenUsingPropertiesExpressionBodyStringInterpolation()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        internal string FullName => $""{this.FirstName} {this.LastName}"";

        internal string FirstName
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

        internal string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        internal string FullName => $""{this.FirstName} {this.LastName}"";

        internal string FirstName
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

        internal string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CalculatedName => this.name == null ? ""Missing"" : this.name;

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                ↓this.name = value;
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CalculatedName => this.name == null ? ""Missing"" : this.name;

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CalculatedName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string FirstName
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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName}"";
            }
        }

        public string FirstName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => $""Hello {this.FullName}"";

        public string FullName
        {
            get
            {
                return $""{this.FirstName}"";
            }
        }

        public string FirstName
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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("=> CreateGreeting()")]
        [TestCase("=> this.CreateGreeting()")]
        public static void WhenUsingPropertiesExpressionCallingMethod(string call)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => CreateGreeting();

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        public string CreateGreeting() => $""Hello {this.FullName}"";

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("=> CreateGreeting()", call);

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => CreateGreeting();

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string FirstName
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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        public string CreateGreeting() => $""Hello {this.FullName}"";

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("=> CreateGreeting()", call);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenUsingPropertiesExpressionBodyTernary()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => this.FirstName == null ? this.LastName : $""{this.FirstName} {this.LastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => this.FirstName == null ? this.LastName : $""{this.FirstName} {this.LastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.FirstName} {this.LastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{FirstName}{LastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{FirstName}{LastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenUsingBackingFieldsExpressionBodyStringInterpolation()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => $""{this.firstName} {this.lastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => $""{this.firstName} {this.lastName}"";

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => string.Format(""{0} {1}"", this.firstName, this.lastName);

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName => string.Format(""{0} {1}"", this.firstName, this.lastName);

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("this.name")]
        [TestCase("name")]
        public static void WhenUsingBackingFieldExpressionBodyStringToUpper(string path)
        {
            var before = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CapsName => this.name.ToUpper();

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                ↓this.name = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.name", path);

            var after = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CapsName => this.name.ToUpper();

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CapsName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.name", path);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenUsingBackingFieldExpressionBodyStringElvisToUpper()
        {
            var before = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CapsName => this.name?.ToUpper();

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                ↓this.name = value;
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
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CapsName => this.name?.ToUpper();

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CapsName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandBox
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
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public Person Person => new Person(this.firstName, this.lastName);

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public Person Person => new Person(this.firstName, this.lastName);

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string[] Names => new[] { this.firstName, this.lastName };

        public string FirstName
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

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Names));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string[] Names => new[] { this.firstName, this.lastName };

        public string FirstName
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
                this.OnPropertyChanged(nameof(this.Names));
            }
        }

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Names));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandBox
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> Names
        {
            get
            {
                yield return this.firstName;
                yield return this.lastName;
            }
        }

        public string FirstName
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

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Names));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandBox
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> Names
        {
            get
            {
                yield return this.firstName;
                yield return this.lastName;
            }
        }

        public string FirstName
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
                this.OnPropertyChanged(nameof(this.Names));
            }
        }

        public string LastName
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
                this.OnPropertyChanged(nameof(this.Names));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.firstName} {this.lastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{this.firstName} {this.lastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CalculatedName
        {
            get
            {
                switch (this.name)
                {
                    case ""Meh"":
                        return ""heM"";
                    default:
                        return this.name;
                }
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                ↓this.name = value;
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CalculatedName
        {
            get
            {
                switch (this.name)
                {
                    case ""Meh"":
                        return ""heM"";
                    default:
                        return this.name;
                }
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CalculatedName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{_firstName} {_lastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FullName
        {
            get
            {
                return $""{_firstName} {_lastName}"";
            }
        }

        public string FirstName
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

        public string LastName
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void FieldUpdatedInMethodWithInvokerInBaseClass()
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var before = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string text;

        public string Text => this.text;

        public void Update(string text)
        {
            ↓this.text = text;
        }
    }
}";

            var after = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string text;

        public string Text => this.text;

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
        public static void InLambda1()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public ViewModel()
        {
            this.PropertyChanged += (o, e) => ↓this.name = this.name + ""meh"";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public ViewModel()
        {
            this.PropertyChanged += (o, e) =>
            {
                this.name = this.name + ""meh"";
                this.OnPropertyChanged(nameof(this.Name));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void InLambda2()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public ViewModel()
        {
            this.PropertyChanged += (o, e) =>
                {
                    ↓this.name = this.name + ""meh"";
                };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public ViewModel()
        {
            this.PropertyChanged += (o, e) =>
                {
                    this.name = this.name + ""meh"";
                    this.OnPropertyChanged(nameof(this.Name));
                };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;
        private int getCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public int GetCount => this.getCount;

        public string Name
        {
            get
            {
                ↓this.getCount++;
                return this.name;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;
        private int getCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public int GetCount => this.getCount;

        public string Name
        {
            get
            {
                this.getCount++;
                this.OnPropertyChanged(nameof(this.GetCount));
                return this.name;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("this.bar.BarValue")]
        [TestCase("bar.BarValue")]
        public static void WhenAssigningNestedField(string path)
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.bar.BarValue;

        public void Update(int value)
        {
            ↓this.bar.BarValue = value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.bar.BarValue", path);
            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.bar.BarValue;

        public void Update(int value)
        {
            this.bar.BarValue = value;
            this.OnPropertyChanged(nameof(this.Value));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.bar.BarValue", path);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
        }

        [Test]
        public static void WhenAssigningNestedFieldRoot()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.bar.BarValue;

        public void Update(int value)
        {
            ↓this.bar = new Bar();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.bar.BarValue;

        public void Update(int value)
        {
            this.bar = new Bar();
            this.OnPropertyChanged(nameof(this.Value));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
        }

        [Test]
        public static void WhenAssigningRootForNestedField()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.bar.BarValue;

        public void Update()
        {
            ↓this.bar = new Bar();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.bar.BarValue;

        public void Update()
        {
            this.bar = new Bar();
            this.OnPropertyChanged(nameof(this.Value));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
        }

        [Test]
        public static void OverriddenProperty()
        {
            var barCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class FooBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract int Value { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var before = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private int value;

        public override int Value => this.value;

        public void Update(int newValue)
        {
            ↓this.value = newValue;
        }
    }
}";
            var after = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private int value;

        public override int Value => this.value;

        public void Update(int newValue)
        {
            this.value = newValue;
            this.OnPropertyChanged(nameof(this.Value));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { barCode, before }, after);
        }

        [Test]
        public static void WhenNotifyingInElseOnly()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class Foo : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        public void Update(bool up)
        {
            if (up)
            {
                ↓this.value++;
            }
            else
            {
                this.value--;
                this.OnPropertyChanged(nameof(this.Value));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class Foo : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value => this.value;

        public void Update(bool up)
        {
            if (up)
            {
                this.value++;
                this.OnPropertyChanged(nameof(this.Value));
            }
            else
            {
                this.value--;
                this.OnPropertyChanged(nameof(this.Value));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
