namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
namespace RoslynSandbox.Core
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
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.TrySet(ref this.value, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, testCode);
            }

            [Test]
            public static void SetWithThisGetWithout()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.TrySet(ref this.value, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, testCode);
            }

            [Test]
            public static void SetExpressionBodies()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int value;

        public int Value
        {
            get => value;
            set => this.TrySet(ref this.value, value);
        }
    }
}";

                RoslynAssert.Valid(Analyzer, ViewModelBaseCode, testCode);
            }

            [TestCase("null")]
            [TestCase("string.Empty")]
            [TestCase(@"""Bar""")]
            [TestCase(@"nameof(Bar)")]
            [TestCase(@"nameof(this.Bar)")]
            public static void RaisePropertyChanged(string propertyName)
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private int bar;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(nameof(Bar));
            }
        }
    }
}".AssertReplace(@"nameof(Bar)", propertyName);

                RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, testCode);
            }
        }
    }
}
