namespace PropertyChangedAnalyzers.Test.INPC009DontRaiseChangeForMissingPropertyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly INPC009DontRaiseChangeForMissingProperty Analyzer = new INPC009DontRaiseChangeForMissingProperty();

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
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar))));
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
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
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
            AnalyzerAssert.Valid(Analyzer, vmCode, testCode);
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
                this.OnPropertyChanged(nameof(this.Value));
            }
        }

        public void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
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
    }
}