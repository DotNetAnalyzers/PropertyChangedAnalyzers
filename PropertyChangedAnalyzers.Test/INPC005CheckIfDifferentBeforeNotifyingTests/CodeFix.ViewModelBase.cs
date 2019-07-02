namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
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

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
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
}";

            [Test]
            public static void NoCheckAddIfReturn()
            {
                var testCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged();
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

        public string Name
        {
            get => this.name;
            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Check that value is different before notifying.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Check that value is different before notifying.");
            }

            [Test]
            public static void NoCheckToUseSetAndRaise()
            {
                var testCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged();
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

        public string Name
        {
            get => this.name;
            set => this.TrySet(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Use ViewModelBase.TrySet");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Use ViewModelBase.TrySet");
            }

            [Test]
            public static void NoCheckExpressionToUseSetAndRaise()
            {
                var testCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged(() => this.Name);
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

        public string Name
        {
            get => this.name;
            set => this.TrySet(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Use ViewModelBase.TrySet");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Use ViewModelBase.TrySet");
            }

            [Test]
            public static void NoIfForTrySet()
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
                this.TrySet(ref this.name, value)
                ↓this.OnPropertyChanged(nameof(this.Greeting));
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
                RoslynAssert.CodeFix<InvocationAnalyzer, CheckIfDifferentBeforeNotifyFix>(ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
                RoslynAssert.FixAll<InvocationAnalyzer, CheckIfDifferentBeforeNotifyFix>(ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
            }

            [Test]
            public static void OutsideIfTrySet()
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
                if (this.TrySet(ref this.name, value))
                {
                }

                ↓this.OnPropertyChanged(nameof(this.Greeting));
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

                RoslynAssert.CodeFix<InvocationAnalyzer, CheckIfDifferentBeforeNotifyFix>(ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
                RoslynAssert.FixAll<InvocationAnalyzer, CheckIfDifferentBeforeNotifyFix>(ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
            }

            [Test]
            public static void InsideIfNegatedTrySet()
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
                if (!this.TrySet(ref this.name, value))
                {
                    ↓this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";

                RoslynAssert.NoFix<InvocationAnalyzer, CheckIfDifferentBeforeNotifyFix>(ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode });
            }

            [Test]
            public static void SetAffectsCalculatedPropertyInternalClassInternalProperty()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    internal class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        internal string Greeting => $""Hello {this.Name}"";

        internal string Name
        {
            get { return this.name; }
            set
            {
                this.TrySet(ref this.name, value)
                ↓this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    internal class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        internal string Greeting => $""Hello {this.Name}"";

        internal string Name
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
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
                if (this.TrySet(ref this.name, value))
                {
                }

                ↓this.OnPropertyChanged(nameof(this.Greeting));
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
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
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                }

                ↓this.OnPropertyChanged(nameof(this.Greeting2));
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
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
                if (this.TrySet(ref this.name, value))
                    this.OnPropertyChanged(nameof(this.Greeting1));
                ↓this.OnPropertyChanged(nameof(this.Greeting2));
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode);
            }
        }
    }
}
