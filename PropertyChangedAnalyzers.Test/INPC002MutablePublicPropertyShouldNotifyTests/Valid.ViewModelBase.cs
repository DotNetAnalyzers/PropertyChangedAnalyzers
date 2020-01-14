namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            [Test]
            public static void Set()
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, code);
            }

            [Test]
            public static void SetWithThisGetWithout()
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P
        {
            get { return p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, code);
            }

            [Test]
            public static void SetExpressionBodies()
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P
        {
            get => p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";

                RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
            }

            [TestCase("null")]
            [TestCase("string.Empty")]
            [TestCase(@"""P""")]
            [TestCase(@"nameof(P)")]
            [TestCase(@"nameof(this.P)")]
            public static void RaisePropertyChanged(string propertyName)
            {
                var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged(nameof(P));
            }
        }
    }
}".AssertReplace(@"nameof(P)", propertyName);

                RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, code);
            }
        }
    }
}
