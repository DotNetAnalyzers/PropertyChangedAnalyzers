// ReSharper disable All
namespace ValidCode.InheritanceTrySet;

public sealed class UnderscoreNames : UnderscoreNamesViewModelBase
{
    private string? _name;
    private int _value;

    public string Greeting => $"Hello {_name}";

    public string? Name
    {
        get => _name;

        set
        {
            if (TrySet(ref _name, value))
            {
                OnPropertyChanged(nameof(Greeting));
            }
        }
    }

    public int Value
    {
        get => _value;
        set => TrySet(ref _value, value);
    }
}
