namespace PropertyChangedAnalyzers.Test.INPC007MissingInvokerTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new EventAnalyzer();
        private static readonly CodeFixProvider Fix = new MissingInvokerFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC007MissingInvoker);

        [Test]
        public static void EventOnlyAddInvoker()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public static void EventOnlyMakeSealed()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Seal class.");
        }

        [Test]
        public static void EventOnlyWithUsing()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public static void EventOnlySealed()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; set; }
    }
}";

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void EventOnlyStatic()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public static class ViewModel
    {
        ↓public static event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void OverridingEvent()
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

            var before = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        ↓public override event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public static void WithNoMutablePropertiesAddInvoker()
        {
            var before = @"
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
            var after = @"
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Add OnPropertyChanged invoker.");
        }

        [Test]
        public static void WithNoMutablePropertiesSeal()
        {
            var before = @"
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
            var after = @"
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Seal class.");
        }

        [Test]
        public static void UsesCorrectStyleIssue107()
        {
            var fooCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.value;
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
    }
}";

            var before = @"
namespace RoslynSandbox
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Foo> Items { get; } = new ObservableCollection<Foo>
        {
            new Foo { Value = 2 },
        };
    }
}";
            var after = @"
namespace RoslynSandbox
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Foo> Items { get; } = new ObservableCollection<Foo>
        {
            new Foo { Value = 2 },
        };

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { fooCode, before }, after, fixTitle: "Add OnPropertyChanged invoker.");
        }
    }
}
