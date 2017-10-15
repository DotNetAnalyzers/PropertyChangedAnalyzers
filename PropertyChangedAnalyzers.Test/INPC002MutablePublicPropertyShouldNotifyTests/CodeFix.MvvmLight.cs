namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class MvvmLight
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly);
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
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
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
                this.RaisePropertyChanged();
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "Notify when value changes.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "Notify when value changes.");
            }

            [Test]
            public void AutoPropertyToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int bar;

        public int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
            }

            [Test]
            public void AutoPropertyInitializedToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        ↓public int Bar { get; set; } = 1;
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int bar = 1;

        public int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
            }

            [Test]
            public void AutoPropertyVirtualToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        ↓public virtual int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int bar;

        public virtual int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
            }

            [Test]
            public void AutoPropertyPrivateSetToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        ↓public int Bar { get; private set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int bar;

        public int Bar { get => this.bar; private set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
            }

            [Test]
            public void AutoPropertyToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        public Foo(int bar)
        {
            Bar = bar;
        }

        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar { get => _bar; set => Set(ref _bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
            }

            [Test]
            public void WithBackingFieldToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
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
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.Set(ref this.name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
            }

            [Test]
            public void WithBackingFieldToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
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
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "ViewModelBase.Set.");
            }
        }
    }
}