namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class StyletMvvm
        {
            private static readonly IReadOnlyList<MetadataReference> MetadataReferences = SpecialMetadataReferences.Stylet;

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
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
    public class C : Stylet.PropertyChangedBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private int p;

        public int P { get => this.p; set => this.SetAndNotify(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void InternalClassInternalPropertyAutoPropertyToTrySet()
            {
                var before = @"
namespace N
{
    internal class C : Stylet.PropertyChangedBase
    {
        internal int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    internal class C : Stylet.PropertyChangedBase
    {
        private int p;

        internal int P { get => this.p; set => this.SetAndNotify(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        public int ↓P { get; set; } = 1;
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private int p = 1;

        public int P { get => this.p; set => this.SetAndNotify(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        public virtual int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private int p;

        public virtual int P { get => this.p; set => this.SetAndNotify(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
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
    public class C : Stylet.PropertyChangedBase
    {
        private int p;

        public int P { get => this.p; private set => this.SetAndNotify(ref this.p, value); }

        public void Mutate()
        {
            this.P++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToTrySetUnderscoreNames()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        public C(int p)
        {
            P = p;
        }

        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private int _p;

        public C(int p)
        {
            P = p;
        }

        public int P { get => _p; set => SetAndNotify(ref _p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string p;

        public string ↓P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string p;

        public string P
        {
            get { return this.p; }
            set { this.SetAndNotify(ref this.p, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetExpressionBody()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string p;

        public string ↓P
        {
            get => this.p;
            set => this.p = value;
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.SetAndNotify(ref this.p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesStatementBody()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string _p;

        public string ↓P
        {
            get { return _p; }
            set { _p = value; }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string _p;

        public string P
        {
            get { return _p; }
            set { SetAndNotify(ref _p, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNames()
            {
                var before = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string _p;

        public string ↓P
        {
            get => _p;
            set => _p = value;
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string _p;

        public string P
        {
            get => _p;
            set => SetAndNotify(ref _p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetAndNotify(ref field, value)", metadataReferences: MetadataReferences);
            }
        }
    }
}
