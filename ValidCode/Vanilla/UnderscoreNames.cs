// ReSharper disable All
namespace ValidCode.Vanilla;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public sealed class UnderscoreNames : INotifyPropertyChanged
{
    private string? _name;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
