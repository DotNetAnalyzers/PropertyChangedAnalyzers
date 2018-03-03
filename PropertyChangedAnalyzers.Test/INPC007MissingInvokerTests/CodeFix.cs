namespace PropertyChangedAnalyzers.Test.INPC007MissingInvokerTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        [Test]
        public void EventOnlyAddInvoker()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(testCode, fixedCode, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public void EventOnlyMakeSealed()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(testCode, fixedCode, fixTitle: "Seal class.");
        }

        [Test]
        public void EventOnlyWithUsing()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(testCode, fixedCode, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public void EventOnlySealed()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; set; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; set; }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(testCode, fixedCode);
        }

        [Test]
        public void EventOnlyStatic()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public static class ViewModel
    {
        ↓public static event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public static class ViewModel
    {
        public static event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(testCode, fixedCode);
        }

        [Test]
        public void OverridingEvent()
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        ↓public override event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(new[] { viewModelBaseCode, testCode }, fixedCode, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public void WithNoMutablePropertiesAddInvoker()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public Foo(int value )
        {
            this.Value = value;
        }

        ↓public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; }

        public int Squared => this.Value * this.Value;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public Foo(int value )
        {
            this.Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; }

        public int Squared => this.Value * this.Value;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(testCode, fixedCode, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public void WithNoMutablePropertiesSeal()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public Foo(int value )
        {
            this.Value = value;
        }

        ↓public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; }

        public int Squared => this.Value * this.Value;
    }
}";
            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class Foo : INotifyPropertyChanged
    {
        public Foo(int value )
        {
            this.Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; }

        public int Squared => this.Value * this.Value;
    }
}";

            AnalyzerAssert.CodeFix<INPC007MissingInvoker, MissingInvokerCodeFix>(testCode, fixedCode, fixTitle: "Seal class.");
        }
    }
}
