namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class StyletMvvm
        {
            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.MetadataReferences.AddRange(SpecialMetadataReferences.Stylet);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetAll();
            }

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.");
            }

            [Test]
            public static void AutoPropertyToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set => this.SetAndNotify(ref this.bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }

            [Test]
            public static void InternalClassInternalPropertyAutoPropertyToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    internal class Foo : Stylet.PropertyChangedBase
    {
        ↓internal int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    internal class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        internal int Bar
        {
            get => this.bar;
            set => this.SetAndNotify(ref this.bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        ↓public int Bar { get; set; } = 1;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar = 1;

        public int Bar
        {
            get => this.bar;
            set => this.SetAndNotify(ref this.bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        ↓public virtual int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        public virtual int Bar
        {
            get => this.bar;
            set => this.SetAndNotify(ref this.bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        ↓public int Bar { get; private set; }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            private set => this.SetAndNotify(ref this.bar, value);
        }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }

            [Test]
            public static void AutoPropertyToSetUnderscoreNames()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        public Foo(int bar)
        {
            Bar = bar;
        }

        ↓public int Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar
        {
            get => _bar;
            set => SetAndNotify(ref _bar, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }

            [Test]
            public static void WithBackingFieldToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        ↓public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetAndNotify(ref this.name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNames()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string _name;

        ↓public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Stylet.PropertyChangedBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetAndNotify(ref _name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "PropertyChangedBase.SetAndNotify.");
            }
        }
    }
}
