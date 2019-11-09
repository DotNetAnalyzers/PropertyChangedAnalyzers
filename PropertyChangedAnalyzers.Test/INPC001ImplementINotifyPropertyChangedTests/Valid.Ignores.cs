namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
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
            public static void GetPrivateSetNeverAssigned()
            {
                var code = @"
namespace N
{
    public class C
    {
        public int P { get; private set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void GetPrivateSetAssignedInConstructorOnly()
            {
                var code = @"
namespace N
{
    public class C
    {
        C(int p)
        {
            this.P = p;
        }

        public int P { get; private set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
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
            public static void StaticEvent()
            {
                // maybe this should notify?
                var code = @"
namespace N
{
    using System.ComponentModel;

    public class KaxamlInfo
    {
        public static event PropertyChangedEventHandler PropertyChanged;
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

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Value
        {
            get { return (int) this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void Event()
            {
                var code = @"
namespace N
{
    using System;

    public class C
    {
        public event EventHandler E;
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void MarkupExtension()
            {
                var code = @"
namespace N
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
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void Attribute()
            {
                var code = @"
namespace N
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class FooAttribute : Attribute
    {
        public string Name { get; set; }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void DataTemplateSelector()
            {
                var code = @"
namespace N
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
                RoslynAssert.Valid(Analyzer, code);
            }

            [Ignore("Not sure if there is a clean way. Not common enough for special casing. Maybe ask for a fix on uservoice :D")]
            [Test]
            public static void WhenBaseHasPropertyChangedEventButNoInterface()
            {
                //// ReSharper disable once HeuristicUnreachableCode
                var code = @"
namespace N
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        public int Foo { get; set; }
    }
}";

                RoslynAssert.Valid(Analyzer, code);
            }

            [TestCase("Value = value;")]
            [TestCase("Value++;")]
            [TestCase("Value--;")]
            public static void PrivateSetterOnlyAssignedInCtor(string expression)
            {
                var code = @"
namespace N
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
}".AssertReplace("Value = value;", expression);

                RoslynAssert.Valid(Analyzer, code);
            }
        }
    }
}
