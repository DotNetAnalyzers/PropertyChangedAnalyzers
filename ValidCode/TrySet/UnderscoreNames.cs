// ReSharper disable All
namespace ValidCode.TrySet
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class UnderscoreNames : INotifyPropertyChanged
    {
        private string _name;
        private int _value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Greeting => $"Hello {_name}";

        public string Name
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
