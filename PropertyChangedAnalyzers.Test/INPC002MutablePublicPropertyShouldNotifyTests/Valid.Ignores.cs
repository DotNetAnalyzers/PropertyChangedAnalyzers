namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Valid
    {
        public static class Ignores
        {
            [Test]
            public static void Struct()
            {
                var code = @"
namespace N
{
    public struct S
    {
        public int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void GetOnly()
            {
                var code = @"
namespace N
{
    public class C
    {
        public int P { get; } = 1;
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void SetOnly()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
namespace N
{
    public class C
    {
        public int P => 1;
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void CalculatedBody()
            {
                var code = @"
namespace N
{
    public class C
    {
        public int P
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
namespace N
{
    public abstract class C
    {
        public abstract int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void Static()
            {
                // maybe this should notify?
                var code = @"
namespace N
{
    public class C
    {
        public static int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void InternalClass()
            {
                // maybe this should notify?
                var code = @"
namespace N
{
    internal class C
    {
        public int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void InternalProperty()
            {
                // maybe this should notify?
                var code = @"
namespace N
{
    public class C
    {
        internal int P { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void DependencyProperty()
            {
                var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class WpfControl : Control
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(WpfControl),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get { return (int) this.GetValue(NumberProperty); }
            set { this.SetValue(NumberProperty, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, Descriptor, code);
            }

            [Test]
            public static void PrivateSetterOnlyAssignedInCtor()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public C(int value)
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
