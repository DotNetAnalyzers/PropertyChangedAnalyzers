namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class ViewModelBaseNotInSource
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = Gu.Roslyn.Asserts.MetadataReferences.FromAttributes()
                                                                                             .Add(Gu.Roslyn.Asserts.MetadataReferences.CreateBinary(@"
namespace N.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}"));

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetPropertyExpressionBodies()
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExplicitNameOf()
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value, nameof(P2)))
                {
                    this.OnPropertyChanged(nameof(P1));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyNameOf()
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(P1));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyStringEmpty()
            {
                var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get => this.p2;
            set => this.TrySet(ref this.p2, value, string.Empty);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void WhenOverriddenSet()
            {
                var viewModelBase = @"
namespace N.Client
{
    public abstract class ViewModelBase : N.Core.ViewModelBase
    {
        protected override bool TrySet<T>(ref T oldValue, T newValue, string propertyName = null)
        {
            return base.TrySet(ref oldValue, newValue, propertyName);
        }
    }
}";

                var code = @"
namespace N.Client
{
    public class C : ViewModelBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, new[] { viewModelBase, code }, metadataReferences: MetadataReferences);
            }
        }
    }
}
