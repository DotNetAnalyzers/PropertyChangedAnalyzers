// ReSharper disable All
namespace ValidCode
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            nameof(Value2),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Value1
        {
            get { return (int)this.GetValue(Value1Property); }
            set { this.SetValue(Value1Property, value); }
        }

        public int Value2
        {
            get => (int)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }
    }
}
