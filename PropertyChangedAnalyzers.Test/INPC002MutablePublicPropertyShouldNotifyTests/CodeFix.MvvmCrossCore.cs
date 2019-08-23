namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class MvvmCrossCore
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmCross;

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToTrySet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public int Bar { get => this.bar; set => this.SetProperty(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void InternalClassInternalPropertyAutoPropertyToTrySet()
            {
                var before = @"
namespace RoslynSandbox
{
    internal class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        internal int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    internal class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        internal int Bar { get => this.bar; set => this.SetProperty(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int ↓Bar { get; set; } = 1;
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar = 1;

        public int Bar { get => this.bar; set => this.SetProperty(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public virtual int ↓Bar { get; set; }
    }
}";

                var after = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public virtual int Bar { get => this.bar; set => this.SetProperty(ref this.bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public int Bar { get => this.bar; private set => this.SetProperty(ref this.bar, value); }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToTrySetUnderscoreNames()
            {
                var before = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class Foo : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar { get => _bar; set => SetProperty(ref _bar, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetExpressionBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNames()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesExpressionBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class ViewModel : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, before }, after, fixTitle: "SetProperty(ref storage, value)", metadataReferences: MetadataReferences);
            }
        }
    }
}
