namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class ViewModelBaseSubclassingPropertyChangedBase
        {
            private const string ViewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    public abstract class ViewModelBase : Caliburn.Micro.PropertyChangedBase
    {
        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            return base.Set(field, newValue, propertyName);
        }
    }
}";

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(Caliburn.Micro.PropertyChangedBase).Assembly);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void AutoPropertyToNotifyWhenValueChanges()
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
                this.OnPropertyChanged();
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Notify when value changes.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Notify when value changes.");
            }

            [Test]
            public void AutoPropertyToSet()
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
            set => this.SetValue(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
            }

            [Test]
            public void AutoPropertyInitailzedToSet()
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
            set => this.SetValue(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
            }

            [Test]
            public void AutoPropertyVirtualToSet()
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
            set => this.SetValue(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
            }

            [Test]
            public void AutoPropertyPrivateSetToSet()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        ↓public int Bar { get; private set; }
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
            private set => this.SetValue(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
            }

            [Test]
            public void AutoPropertyToSetUnderscoreNames()
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
            set => SetValue(ref _bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
            }

            [Test]
            public void WithBackingFieldToSet()
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
            set { this.SetValue(ref this.name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
            }

            [Test]
            public void WithBackingFieldToSetUnderscoreNames()
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
            set { SetValue(ref _name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "ViewModelBase.SetValue.");
            }

            [Test]
            public void AutoPropertyWhenRecursionInSetValue()
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

        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            return this.SetValue(ref field, newValue, propertyName);
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
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { viewModelBaseCode, testCode }, fixedCode);
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(new[] { viewModelBaseCode, testCode }, fixedCode);
            }
        }
    }
}