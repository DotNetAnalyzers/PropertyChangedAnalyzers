// ReSharper disable All
namespace ValidCode.Inheritance;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class StatementBodiesViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
