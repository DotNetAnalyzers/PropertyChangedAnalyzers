// ReSharper disable All
namespace ValidCode.Inheritance;

public sealed class UnderscoreNames : UnderscoreNamesViewModelBase
{
    private string? _name;

    public string Greeting => $"Hello {_name}";

    public string? Name
    {
        get => _name;

        set
        {
            if (value == _name)
            {
                return;
            }

            _name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Greeting));
        }
    }
}
