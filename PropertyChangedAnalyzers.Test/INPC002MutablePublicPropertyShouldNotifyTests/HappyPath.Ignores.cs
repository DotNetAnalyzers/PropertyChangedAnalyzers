namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        internal class Ignores
        {
            [Test]
            public void InvokesCached()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs BarPropertyChangedArgs = new PropertyChangedEventArgs(nameof(Bar));
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, BarPropertyChangedArgs);
            }
        }
    }
}";
                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreStruct()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public struct Foo
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreGetOnly()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar { get; } = 1;
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreSetOnly()
            {
                var testCode = @"
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

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreExpressionBody()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar => 1;
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreCalculatedBody()
            {
                var testCode = @"
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

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreAbstract()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public abstract class Foo
    {
        public abstract int Bar { get; set; }
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreStatic()
            {
                // maybe this should notify?
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public static int Bar { get; set; }
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreInternalClass()
            {
                // maybe this should notify?
                var testCode = @"
namespace RoslynSandbox
{
    internal class Foo
    {
        public int Bar { get; set; }
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreInternalProperty()
            {
                // maybe this should notify?
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        internal int Bar { get; set; }
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void IgnoreDependencyProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int) this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";
                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }
        }
    }
}