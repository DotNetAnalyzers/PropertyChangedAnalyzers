// ReSharper disable All
namespace ValidCode;

using System.Windows;
using System.Windows.Controls;

public class CustomControl : Control
{
    public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
        nameof(Value1),
        typeof(int),
        typeof(CustomControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
        nameof(Value2),
        typeof(int),
        typeof(CustomControl),
        new PropertyMetadata(default(int)));

    public int Value1
    {
#pragma warning disable INPC020 // Prefer expression body accessor.
        get { return (int)this.GetValue(Value1Property); }
        set { this.SetValue(Value1Property, value); }
#pragma warning restore INPC020 // Prefer expression body accessor.
    }

    public int Value2
    {
        get => (int)this.GetValue(Value2Property);
        set => this.SetValue(Value2Property, value);
    }
}
