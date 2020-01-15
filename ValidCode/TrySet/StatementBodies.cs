// ReSharper disable All
#pragma warning disable INPC020 // Prefer expression body accessor.
namespace ValidCode.TrySet
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class StatementnBodies : INotifyPropertyChanged
    {
        private string? name;
        private int value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Greeting
        {
            get
            {
                return $"Hello {this.name}";
            }
        }

        public string? Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.TrySet(ref this.value, value);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
