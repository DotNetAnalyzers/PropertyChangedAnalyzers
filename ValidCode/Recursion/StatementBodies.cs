// ReSharper disable All
#pragma warning disable INPC007, INPC015, INPC020, CS0067
namespace ValidCode.Recursion
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class StatementnBodies : INotifyPropertyChanged
    {
        private string? name;
        private int value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Greeting1
        {
            get
            {
                return this.Greeting1;
            }
        }

        public string Greeting2
        {
            get
            {
                return $"{this.Name}";
            }
        }

        public string? Name
        {
            get
            {
                return this.Name;
            }

            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                    this.OnPropertyChanged(nameof(this.Greeting2));
                }
            }
        }

        public int Value
        {
            get
            {
                return this.Value;
            }

            set
            {
                this.TrySet(ref this.value, value);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.OnPropertyChanged(propertyName);
        }

        private bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            return this.TrySet(ref field, newValue, propertyName);
        }
    }
}
