namespace PropertyChangedAnalyzers.Test.INPC014PreferSettingBackingFieldInCtorTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
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

        [Test]
        public void WhenSettingBackingField()
        {
            var testCode = @"
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

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<INPC014PreferSettingBackingFieldInCtor, SetBackingFieldCodeFix>(testCode, fixedCode);
        }

        [Test]
        public void WhenSettingBackingFieldUnderscoreNames()
        {
            var testCode = @"
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

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<INPC014PreferSettingBackingFieldInCtor, SetBackingFieldCodeFix>(testCode, fixedCode);
        }

        [Test]
        public void WhenSettingBackingFieldInNotifyingProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            ↓this.Value = value;
        }

        [DataMember]
        public int Value
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
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            this.value = value;
        }

        [DataMember]
        public int Value
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
}";
            AnalyzerAssert.CodeFix<INPC014PreferSettingBackingFieldInCtor, SetBackingFieldCodeFix>(testCode, fixedCode);
        }

        [Test]
        public void WhenSettingFieldUsingTrySet()
        {
            var testCode = @"
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
            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<INPC014PreferSettingBackingFieldInCtor, SetBackingFieldCodeFix>(new[] { ViewModelBaseCode, testCode }, fixedCode);
        }

        [Test]
        public void WhenSettingFieldUsingTrySetAndNotifyForOther()
        {
            var testCode = @"
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
            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<INPC014PreferSettingBackingFieldInCtor, SetBackingFieldCodeFix>(new[] { ViewModelBaseCode, testCode }, fixedCode);
        }
    }
}
