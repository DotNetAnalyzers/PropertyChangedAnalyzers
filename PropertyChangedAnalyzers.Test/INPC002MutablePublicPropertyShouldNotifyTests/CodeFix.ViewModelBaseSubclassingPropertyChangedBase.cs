namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class ViewModelBaseInheritsPropertyChangedBase
        {
            private const string ViewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : Caliburn.Micro.PropertyChangedBase
    {
        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            return base.Set(ref field, value, propertyName);
        }
    }
}";

            private const string ViewModelBaseUnderscore = @"
namespace RoslynSandbox
{
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : Caliburn.Micro.PropertyChangedBase
    {
        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            return base.Set(ref field, value, propertyName);
        }

        int M() => M();
    }
}";

            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.CaliburnMicro;

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public int ↓Bar { get; set; }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Notify when value changes.", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToSet()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public int ↓Bar { get; set; }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public int ↓Bar { get; set; } = 1;
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public virtual int ↓Bar { get; set; }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public int ↓Bar { get; private set; }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void AutoPropertyToSetUnderscoreNames()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public Foo(int bar)
        {
            Bar = bar;
        }

        public int ↓Bar { get; set; }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnderScoreFieldsUnqualified, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetExpressionBody()
            {
                var before = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
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
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.TrySet(ref this.name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesStatementBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : RoslynSandbox.ViewModelBase
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
    public class ViewModel : RoslynSandbox.ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { TrySet(ref _name, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesExpressionBody()
            {
                var before = @"
namespace RoslynSandbox
{
    public class ViewModel : ViewModelBase
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
    public class ViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => TrySet(ref _name, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", metadataReferences: MetadataReferences);
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

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            return this.TrySet(ref field, value, propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var before = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        public int ↓Bar { get; set; }
    }
}";

                var after = @"
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, metadataReferences: MetadataReferences);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, metadataReferences: MetadataReferences);
            }
        }
    }
}
