namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class ValidCode
    {
        internal class Ignores
        {
            [Test]
            public void Struct()
            {
                var testCode = @"
namespace RoslynSandBox
{
    public struct Foo
    {
        public int Bar { get; set; }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void GetOnly()
            {
                var testCode = @"
namespace RoslynSandBox
{
    public class Foo
    {
        public int Bar { get; } = 1;
    }
}";

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void ExpressionBody()
            {
                var testCode = @"
namespace RoslynSandBox
{
    public class Foo
    {
        public int Bar => 1;
    }
}";

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void CalculatedBody()
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

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void Abstract()
            {
                var testCode = @"
namespace RoslynSandBox
{
    public abstract class Foo
    {
        public abstract int Bar { get; set; }
    }
}";

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void Static()
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

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void StaticEvent()
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

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void DependencyProperty()
            {
                var testCode = @"
namespace RoslynSandBox
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void Event()
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void MarkupExtension()
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void Attribute()
            {
                var testCode = @"
namespace RoslynSandBox
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class FooAttribute : Attribute
    {
        public string Name { get; set; }
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void DataTemplateSelector()
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
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void WhenBaseHasPropertyChangedEventButNoInterface()
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

                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [TestCase("Value = value;")]
            [TestCase("Value++;")]
            [TestCase("Value--;")]
            public void PrivateSetterOnlyAssignedInCtor(string code)
            {
                var testCode = @"
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
}".AssertReplace("Value = value;", code);

                AnalyzerAssert.Valid(Analyzer, testCode);
            }
        }
    }
}
