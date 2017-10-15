namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class CaliburnMicro
        {
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
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int bar;

        public int Bar
        {
            get
            {
                return this.bar;
            }

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
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "Notify when value changes.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "Notify when value changes.");
            }

            [Test]
            public void AutoPropertyToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int bar;

        public int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
            }

            [Test]
            public void AutoPropertyInitializedToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        ↓public int Bar { get; set; } = 1;
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int bar = 1;

        public int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
            }

            [Test]
            public void AutoPropertyVirtualToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        ↓public virtual int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int bar;

        public virtual int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
            }

            [Test]
            public void AutoPropertyPrivateSetToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        ↓public int Bar { get; private set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int bar;

        public int Bar { get => this.bar; private set => this.Set(ref this.bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
            }

            [Test]
            public void AutoPropertyToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
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
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar { get => _bar; set => Set(ref _bar, value); }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
            }

            [Test]
            public void WithBackingFieldToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
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
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.Set(ref this.name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
            }

            [Test]
            public void WithBackingFieldToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
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
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode, "PropertyChangedBase.Set.");
            }
        }
    }
}