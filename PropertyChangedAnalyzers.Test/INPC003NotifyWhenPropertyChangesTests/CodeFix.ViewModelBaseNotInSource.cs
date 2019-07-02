namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ViewModelBaseNotInSource
        {
            [OneTimeSetUp]
            public static void OneTimeSetup()
            {
                RoslynAssert.MetadataReferences.Add(MetadataReferences.CreateBinary(@"
namespace RoslynSandbox.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
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
}"));
            }

            [OneTimeTearDown]
            public static void OneTimeTearDown()
            {
                RoslynAssert.ResetAll();
            }

            [Test]
            public static void AssignedAffectsCalculatedPropertyOnPropertyChanged()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string firstName;
        private string lastName;

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
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string firstName;
        private string lastName;

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
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void IfNotSetReturnSetAffectsSecondCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (!this.TrySet(↓ref this.name, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.Greeting1));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (!this.TrySet(ref this.name, value))
                {
                    return;
                }
                
                this.OnPropertyChanged(nameof(this.Greeting1));
                this.OnPropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void IfNotSetReturnSetAffectsSecondCalculatedPropertyNoBraces()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (!this.TrySet(↓ref this.name, value))
                    return;
                
                this.OnPropertyChanged(nameof(this.Greeting1));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (!this.TrySet(ref this.name, value))
                    return;
                
                this.OnPropertyChanged(nameof(this.Greeting1));
                this.OnPropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set { this.TrySet(↓ref this.name, value); }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionBodyGetter()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting
        {
            get => $""Hello {this.Name}"";
        }

        public string Name
        {
            get { return this.name; }
            set { this.TrySet(↓ref this.name, value); }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting
        {
            get => $""Hello {this.Name}"";
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void SetExpressionBodiesAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get => this.name;
            set => this.TrySet(↓ref this.name, value);
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get => this.name;
            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(↓ref this.name, value))
                {
                }
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void SetAffectsSecondCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(↓ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                }
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                    this.OnPropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(↓ref this.name, value))
                    this.OnPropertyChanged(nameof(this.Greeting1));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                    this.OnPropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }
        }
    }
}
