namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class Ignores
        {
            [Test]
            public static void Struct()
            {
                var code = @"
namespace RoslynSandbox
{
    public struct Foo
    {
        public int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void GetOnly()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar { get; } = 1;
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void SetOnly()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int writeOnly;

        public event PropertyChangedEventHandler PropertyChanged;

        public int WriteOnly
        {
            set
            {
                this.writeOnly = value;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void ExpressionBody()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar => 1;
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void CalculatedBody()
            {
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar
        {
            get { return 1; }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void Abstract()
            {
                var code = @"
namespace RoslynSandbox
{
    public abstract class Foo
    {
        public abstract int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void Static()
            {
                // maybe this should notify?
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public static int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void InternalClass()
            {
                // maybe this should notify?
                var code = @"
namespace RoslynSandbox
{
    internal class Foo
    {
        public int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void InternalProperty()
            {
                // maybe this should notify?
                var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        internal int Bar { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void DependencyProperty()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int) this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void PrivateSetterOnlyAssignedInCtor()
            {
                var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            Value = value;
        }

        [DataMember]
        public int Value { get; private set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }
        }
    }
}
