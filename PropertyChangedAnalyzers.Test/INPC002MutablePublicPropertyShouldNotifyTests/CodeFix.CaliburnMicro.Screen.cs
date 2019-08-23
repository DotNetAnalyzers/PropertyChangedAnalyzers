namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class CaliburnMicroScreen
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.CaliburnMicro;

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        public int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToTrySet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        public int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        private int bar;

        public int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        public int ↓Bar { get; set; } = 1;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        private int bar = 1;

        public int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        public virtual int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        private int bar;

        public virtual int Bar { get => this.bar; set => this.Set(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        public int ↓Bar { get; private set; }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        private int bar;

        public int Bar { get => this.bar; private set => this.Set(ref this.bar, value); }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToTrySetUnderscoreNames()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        public Foo(int bar)
        {
            Bar = bar;
        }

        public int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.Screen
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar { get => _bar; set => Set(ref _bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string name;

        public string ↓Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.Set(ref this.name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetExpressionBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string name;

        public string ↓Name
        {
            get => this.name;
            set => this.name = value;
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.Set(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesStatementBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string _name;

        public string ↓Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesExpressionBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string _name;

        public string ↓Name
        {
            get => _name;
            set => _name = value;
        }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.Screen
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }
        }
    }
}
