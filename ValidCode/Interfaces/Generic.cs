// ReSharper disable All
namespace ValidCode.Interfaces;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Generic<T> : IValue, INotifyPropertyChanged
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private T value;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public event PropertyChangedEventHandler? PropertyChanged;

    public T Value
    {
        get => this.value;
        set
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, this.value))
            {
                return;
            }

            this.value = value;
            this.OnPropertyChanged();
        }
    }

    object? IValue.Value
    {
        get => this.value;
#pragma warning disable CS8601 // Possible null reference assignment.
        set => this.Value = (T?)value;
#pragma warning restore CS8601 // Possible null reference assignment.
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
