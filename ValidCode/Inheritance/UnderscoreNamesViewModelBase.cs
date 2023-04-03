// ReSharper disable All
namespace ValidCode.Inheritance;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class UnderscoreNamesViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
