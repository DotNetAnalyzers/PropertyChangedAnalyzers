namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ViewModelBaseSubclassingPropertyChangedBase
        {
            private const string ViewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : Caliburn.Micro.PropertyChangedBase
    {
        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            return base.Set(ref field, newValue, propertyName);
        }
    }
}";

            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.AddTransitiveMetadataReferences(typeof(Caliburn.Micro.PropertyChangedBase).Assembly);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetAll();
            }

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.NotifyOfPropertyChange();
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Notify when value changes.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "Notify when value changes.");
            }

            [Test]
            public static void AutoPropertyToSet()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set => this.TrySet(ref this.bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        ↓public int Bar { get; set; } = 1;
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar = 1;

        public int Bar
        {
            get => this.bar;
            set => this.TrySet(ref this.bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        ↓public virtual int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public virtual int Bar
        {
            get => this.bar;
            set => this.TrySet(ref this.bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        ↓public int Bar { get; private set; }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            private set => this.TrySet(ref this.bar, value);
        }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
            }

            [Test]
            public static void AutoPropertyToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public Foo(int bar)
        {
            Bar = bar;
        }

        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar
        {
            get => _bar;
            set => TrySet(ref _bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
            }

            [Test]
            public static void WithBackingFieldToSet()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        ↓public string Name
        {
            get { return this.name; }
            set { this.name = value; }
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
            get { return this.name; }
            set { this.TrySet(ref this.name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string _name;

        ↓public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { TrySet(ref _name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, testCode }, fixedCode, fixTitle: "ViewModelBase.TrySet.");
            }

            [Test]
            public static void AutoPropertyWhenRecursionInTrySet()
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
            return this.TrySet(ref field, newValue, propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, testCode }, fixedCode);
            }
        }
    }
}
