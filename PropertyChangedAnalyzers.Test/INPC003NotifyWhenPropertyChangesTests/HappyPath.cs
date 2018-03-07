namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        private static readonly INPC003NotifyWhenPropertyChanges Analyzer = new INPC003NotifyWhenPropertyChanges();

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void NoCalculated(string propertyName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(nameof(Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenNotifyingCallerMemberName()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsOnPropertyChangedCopyLocalNullCheckInvoke()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsOnPropertyChangedWithExpression()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged(() => this.Value);
            }
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ExpressionInvokerCalculatedProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
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
                this.OnPropertyChanged(() => this.FirstName);
                this.OnPropertyChanged(() => this.FullName);
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
                this.OnPropertyChanged(() => this.LastName);
                this.OnPropertyChanged(() => this.FullName);
            }
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenNotifyingCallerMemberNameExpressionBody()
        {
            var testCode = @"
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenNotifyingMvvmFramework()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using MvvmFramework;

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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsOnPropertyChangedWithCachedEventArgs()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs FirstNameArgs = new PropertyChangedEventArgs(nameof(FirstName));
        private static readonly PropertyChangedEventArgs LastNameArgs = new PropertyChangedEventArgs(nameof(LastName));
        private static readonly PropertyChangedEventArgs FullNameArgs = new PropertyChangedEventArgs(nameof(FullName));

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
                this.OnPropertyChanged(FirstNameArgs);
                this.OnPropertyChanged(FullNameArgs);
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
                this.OnPropertyChanged(LastNameArgs);
                this.OnPropertyChanged(FullNameArgs);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsChainedOnPropertyChanged()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private string meh;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Meh
        {
            get
            {
                return this.meh;
            }

            set
            {
                if (value == this.meh)
                {
                    return;
                }

                this.meh = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenNotifyingSettingFieldInMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void UpdateName(string name)
        {
            this.name = name;
            this.OnPropertyChanged(nameof(Name));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenNotifyingSettingFieldInMethodOutsideLock()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private readonly object gate = new object();
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void UpdateName(string name)
        {
            lock (this.gate)
            {
                this.name = name;
            }

            this.OnPropertyChanged(nameof(Name));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void NotifyingInLambda()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("\"\"")]
        [TestCase("string.Empty")]
        [TestCase("null")]
        public void NotifyThatAllPropertiesChanges(string arg)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;
    using System.ComponentModel;
    public class Foo : INotifyPropertyChanged
    {
        private Dictionary<int, int> map;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value1 => map?[1] ?? 0;

        public int Value2 => map?[2] ?? 0;

        public void Update(Dictionary<int, int> newMap)
        {
            this.map = newMap;
            this.OnPropertyChanged(string.Empty);
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            testCode = testCode.AssertReplace("string.Empty", arg);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void NoFieldTouched()
        {
            //// This test is mostly for debugging when optimizing avoiding using syntax model.
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private string name;

        public void Bar(int a)
        {
            var temp = 1;
            temp++;
            for (var i = 0; i < 10; i++)
            {
                
            }

            temp = 2;
            a = temp;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => this.name;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
