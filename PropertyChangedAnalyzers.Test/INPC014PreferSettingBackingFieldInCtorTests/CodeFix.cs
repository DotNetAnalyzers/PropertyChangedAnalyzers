namespace PropertyChangedAnalyzers.Test.INPC014PreferSettingBackingFieldInCtorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AssignmentAnalyzer();
        private static readonly CodeFixProvider Fix = new SetBackingFieldFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(INPC014SetBackingField.Descriptor);

#pragma warning disable SA1203 // Constants must appear before fields
        private const string ViewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
#pragma warning restore SA1203 // Constants must appear before fields

        [Test]
        public static void SimplePropertyWithBackingFieldStatementBodySetter()
        {
            var before = @"
namespace RoslynSandbox
{
    public class ViewModel
    {
        private int value;

        public ViewModel(int value)
        {
            ↓this.Value = value;
        }

        public int Value
        {
            get => this.value;
            private set
            {
                this.value = value;
            }
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class ViewModel
    {
        private int value;

        public ViewModel(int value)
        {
            this.value = value;
        }

        public int Value
        {
            get => this.value;
            private set
            {
                this.value = value;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SimplePropertyWithBackingFieldExpressionBodySetter()
        {
            var before = @"
namespace RoslynSandbox
{
    public class ViewModel
    {
        private int value;

        public ViewModel(int value)
        {
            ↓this.Value = value;
        }

        public int Value
        {
            get => this.value;
            private set => this.value = value;
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class ViewModel
    {
        private int value;

        public ViewModel(int value)
        {
            this.value = value;
        }

        public int Value
        {
            get => this.value;
            private set => this.value = value;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SimplePropertyWithBackingFieldUnderscoreNames()
        {
            var before = @"
namespace RoslynSandbox
{
    public class ViewModel
    {
        private int _value;

        public ViewModel(int value)
        {
            ↓Value = value;
        }

        public int Value
        {
            get => _value;
            private set
            {
                _value = value;
            }
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class ViewModel
    {
        private int _value;

        public ViewModel(int value)
        {
            _value = value;
        }

        public int Value
        {
            get => _value;
            private set
            {
                _value = value;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("value == this.value")]
        [TestCase("this.value == value")]
        [TestCase("Equals(this.value, value)")]
        [TestCase("Equals(value, this.value)")]
        [TestCase("ReferenceEquals(this.value, value)")]
        [TestCase("ReferenceEquals(value, this.value)")]
        [TestCase("value.Equals(this.value)")]
        [TestCase("this.value.Equals(value)")]
        public static void NotifyingProperty(string equals)
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        private string value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(string value)
        {
            ↓this.Value = value;
        }

        [DataMember]
        public string Value
        {
            get => this.value;
            private set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("value == this.value", equals);

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        private string value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(string value)
        {
            this.value = value;
        }

        [DataMember]
        public string Value
        {
            get => this.value;
            private set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("value == this.value", equals);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenSettingFieldUsingTrySet()
        {
            var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public Foo(int bar)
        {
            ↓this.Bar = bar;
        }
        
        public int Bar
        {
            get => this.bar;
            set => this.TrySet(ref this.bar, value);
        }
    }
}";
            var after = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public Foo(int bar)
        {
            this.bar = bar;
        }
        
        public int Bar
        {
            get => this.bar;
            set => this.TrySet(ref this.bar, value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
        }

        [Test]
        public static void WhenSettingFieldUsingTrySetAndNotifyForOther()
        {
            var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public Foo(string name)
        {
            ↓this.Name = name;
        }
        
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
            var after = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public Foo(string name)
        {
            this.name = name;
        }
        
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
        }

        [Test]
        public static void WhenShadowingParameter()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class A : INotifyPropertyChanged
    {
        public A(bool x)
        {
            ↓X = x;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

    public class A : INotifyPropertyChanged
    {
        public A(bool x)
        {
            this.x = x;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
        }

        [Test]
        public static void WhenShadowingLocal()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class A : INotifyPropertyChanged
    {
        public A(bool a)
        {
            var x = a;
            ↓X = a;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

    public class A : INotifyPropertyChanged
    {
        public A(bool a)
        {
            var x = a;
            this.x = a;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
        }
    }
}
