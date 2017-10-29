namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void CallsOnPropertyChanged(string propertyName)
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void CallsRaisePropertyChangedWithEventArgs(string propertyName)
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void CallsRaisePropertyChangedCallerMemberName()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void Invokes(string propertyName)
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar))));
            }
        }
    }
}";
            testCode = testCode.AssertReplace(@"nameof(this.Bar))", propertyName);
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void InvokesCached()
        {
            var testCode = @"
namespace RoslynSandBox
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
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreStruct()
        {
            var testCode = @"
namespace RoslynSandBox
{
    public struct Foo
    {
        public int Bar { get; set; }
    }
}";
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreGetOnly()
        {
            var testCode = @"
namespace RoslynSandBox
{
    public class Foo
    {
        public int Bar { get; } = 1;
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreExpressionBody()
        {
            var testCode = @"
namespace RoslynSandBox
{
    public class Foo
    {
        public int Bar => 1;
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreCalculatedBody()
        {
            var testCode = @"
namespace RoslynSandBox
{
    public class Foo
    {
        public int Bar
        {
            get { return 1; }
        }
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreAbstract()
        {
            var testCode = @"
namespace RoslynSandBox
{
    public abstract class Foo
    {
        public abstract int Bar { get; set; }
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreStatic()
        {
            // maybe this should notify?
            var testCode = @"
namespace RoslynSandBox
{
    public class Foo
    {
        public static int Bar { get; set; }
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreStaticEvent()
        {
            // maybe this should notify?
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class KaxamlInfo
    {
        public static event PropertyChangedEventHandler PropertyChanged;
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoreDependencyProperty()
        {
            var testCode = @"
namespace RoslynSandBox
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
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoresEvent()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System;

    public class Foo
    {
        public event EventHandler foo;
    }
}";
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoresMarkupExtension()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility WhenTrue { get; set; } = Visibility.Visible;

        public Visibility WhenFalse { get; set; } = Visibility.Collapsed;

        public Visibility WhenNull { get; set; } = Visibility.Collapsed;

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return this.WhenNull;
            }

            if (Equals(value, true))
            {
                return this.WhenTrue;
            }

            if (Equals(value, false))
            {
                return this.WhenFalse;
            }

            throw new ArgumentOutOfRangeException(nameof(value), value, ""Expected value to be of type bool or Nullable<bool>"");
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($""{nameof(BooleanToVisibilityConverter)} is only for OneWay bindings"");
        }
    }
}";
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoresDataTemplateSelector()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    [SuppressMessage(""ReSharper"", ""MemberCanBePrivate.Global"", Justification = ""Used from xaml"")]
    public class DialogButtonTemplateSelector : DataTemplateSelector
    {
        public DataTemplate OKTemplate { get; set; }

        public DataTemplate CancelTemplate { get; set; }

        public DataTemplate YesTemplate { get; set; }

        public DataTemplate NoTemplate { get; set; }

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var result = item as MessageBoxResult?;
            if (!result.HasValue)
            {
                return base.SelectTemplate(item, container);
            }

            switch (result.Value)
            {
                case MessageBoxResult.None:
                    return base.SelectTemplate(item, container);
                case MessageBoxResult.OK:
                    return this.OKTemplate;
                case MessageBoxResult.Cancel:
                    return this.CancelTemplate;
                case MessageBoxResult.Yes:
                    return this.YesTemplate;
                case MessageBoxResult.No:
                    return this.NoTemplate;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}";
            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }

        [Test]
        public void IgnoresWhenBaseHasPropertyChangedEventButNoInterface()
        {
            Assert.Inconclusive("Not sure if there is a clean way. Not common enough for special casing. Maybe ask for a fix on uservoice :D");
            //// ReSharper disable once HeuristicUnreachableCode
            var testCode = @"
namespace RoslynSandBox
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        public int Foo { get; set; }
    }
}";

            AnalyzerAssert.Valid<INPC001ImplementINotifyPropertyChanged>(testCode);
        }
    }
}