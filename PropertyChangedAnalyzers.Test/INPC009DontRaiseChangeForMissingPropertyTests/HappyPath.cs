namespace PropertyChangedAnalyzers.Test.INPC009DontRaiseChangeForMissingPropertyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [TestFixture(typeof(ArgumentAnalyzer))]
    [TestFixture(typeof(InvocationAnalyzer))]
    internal class HappyPath<T>
        where T : DiagnosticAnalyzer, new()
    {
        private static readonly T Analyzer = new T();
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor);

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void OnPropertyChangedWithEventArgs(string propertyName)
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
#pragma warning disable INPC013
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
#pragma warning restore INPC013
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void OnPropertyChangedCallerMemberName()
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
                this.OnPropertyChanged();
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
        public void OnPropertyChangedCallerMemberNameCopyLocalNullCheckInvoke()
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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void OnPropertyChangedCallerMemberNameCopyLocalNullCheckImplicitInvoke()
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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void Invokes(string propertyName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
#pragma warning disable INPC013
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar))));
#pragma warning restore INPC013
            }
        }
    }
}";
            testCode = testCode.AssertReplace(@"nameof(this.Bar))", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void InvokesCached()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs BarPropertyChangedArgs = new PropertyChangedEventArgs(nameof(Bar));
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, BarPropertyChangedArgs);
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void OnPropertyChangedInBaseClass()
        {
            var vmCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var testCode = @"
namespace RoslynSandBox
{
    public class ViewModel : ViewModelBase
    {
        private int value;

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
                this.OnPropertyChanged(nameof(this.Value));
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic, vmCode, testCode);
        }

        [Test]
        public void RaisesForIndexer()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : Collection<int>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void SetItem(int index, int item)
        {
            base.SetItem(index, item);
            this.OnPropertyChanged(""Item[]"");
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
        public void IgnoresWhenNotInvokingReproIssue122()
        {
            var extCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public static class PropertyChangedEventArgsExt
    {
        public static bool HasPropertyChanged(this PropertyChangedEventArgs e, string propertyName)
        {
            return string.Equals(e.PropertyName, propertyName);
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public ViewModel()
        {
            this.PropertyChanged += OnPropertyChanged;
        }

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.HasPropertyChanged(""SomeProperty""))
            {
                // do something
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, extCode, testCode);
        }

        [Test]
        public void RaiseForOtherInstance()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaiseForChild(string propertyName)
        {
            var vm = new ViewModel();
            vm.OnPropertyChanged(propertyName);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void RaiseForOtherInstanceOfOtherType()
        {
            var vmCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
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
                this.OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName]string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var testCode = @"
namespace RoslynSandBox
{
    internal class Foo
    {
        public void Bar()
        {
            var vm = new ViewModel();
            vm.OnPropertyChanged(""Value"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, vmCode, testCode);
        }

        [Test]
        public void RaiseForOtherInstanceOfOtherTypeWithBaseClass()
        {
            var vmBaseCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var vmCode = @"
namespace RoslynSandBox
{
    public class ViewModel : ViewModelBase
    {
        private int value;

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
                this.OnPropertyChanged(nameof(this.Value));
            }
        }
    }
}";

            var testCode = @"
namespace RoslynSandBox
{
    internal class Foo
    {
        public void Bar()
        {
            var vm = new ViewModel();
            vm.OnPropertyChanged(""Value"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, vmBaseCode, vmCode, testCode);
        }

        [Test]
        public void WhenNotAnInvoker()
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
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(""Missing"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
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
        public void WithViewModelBase()
        {
            var viewModelBaseCode = @"
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
            var viewModelCode = @"
namespace RoslynSandbox.Client
{
    using RoslynSandbox.Core;

    public class ViewModel : ViewModelBase
    {
        private int value;
        private int value2;

        public int Sum => this.Value + this.Value2;

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
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Sum));
            }
        }

        public int Value2
        {
            get => this.value2;
            set
            {
                if (this.TrySet(ref this.value2, value))
                {
                    this.OnPropertyChanged(nameof(this.Sum));
                }
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, viewModelBaseCode, viewModelCode);
        }

        [Test]
        public void OverriddenProperty()
        {
            var fooBase = @"
namespace RoslynSandbox.Core
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
            var foo = @"
namespace RoslynSandbox.Client
{
    using RoslynSandbox.Core;

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

            AnalyzerAssert.Valid(Analyzer, fooBase, foo);
        }
    }
}
