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
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private int p;

        public int P { get => this.p; set => this.Set(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        public int ↓P { get; set; } = 1;
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private int p = 1;

        public int P { get => this.p; set => this.Set(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        public virtual int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private int p;

        public virtual int P { get => this.p; set => this.Set(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        public int ↓P { get; private set; }

        public void Mutate()
        {
            this.P++;
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private int p;

        public int P { get => this.p; private set => this.Set(ref this.p, value); }

        public void Mutate()
        {
            this.P++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Set(ref oldValue, newValue)", metadataReferences: MetadataReferences);
            }

            [TestCaseSource(typeof(Code), nameof(Code.AutoDetectedStyles))]
            public static void AutoPropertyToTrySetStyleDetection(AutoDetectedStyle style)
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private int p;

        public int P { get => this.p; set => Set(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(
                    Analyzer,
                    Fix,
                    ExpectedDiagnostic,
                    new[] { style.AdditionalSample, before },
                    style.Apply(after, "p"),
                    fixTitle: "Set(ref oldValue, newValue)",
                    metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
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
namespace N
{
    public class C : Caliburn.Micro.Screen
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
namespace N
{
    public class C : Caliburn.Micro.Screen
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
namespace N
{
    public class C : Caliburn.Micro.Screen
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

            [TestCaseSource(typeof(Code), nameof(Code.AutoDetectedStyles))]
            public static void WithBackingFieldToSetStatementBodyStyleDetection(AutoDetectedStyle style)
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
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
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { Set(ref this.name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(
                    Analyzer,
                    Fix,
                    ExpectedDiagnostic,
                    new[] { style.AdditionalSample, style.Apply(before, "name") },
                    style.Apply(after, "name"),
                    fixTitle: "Set(ref oldValue, newValue)",
                    metadataReferences: MetadataReferences);
            }

            [TestCaseSource(typeof(Code), nameof(Code.AutoDetectedStyles))]
            public static void WithBackingFieldToSetExpressionBodyStyleDetection(AutoDetectedStyle style)
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.Screen
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
namespace N
{
    public class C : Caliburn.Micro.Screen
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => Set(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(
                    Analyzer,
                    Fix,
                    ExpectedDiagnostic,
                    new[] { style.AdditionalSample, before },
                    after,
                    fixTitle: "Set(ref oldValue, newValue)",
                    metadataReferences: MetadataReferences);
            }
        }
    }
}
