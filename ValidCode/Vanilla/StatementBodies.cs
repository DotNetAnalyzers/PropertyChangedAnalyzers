// ReSharper disable All
#pragma warning disable INPC020 // Prefer expression body accessor.
namespace ValidCode.Vanilla
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class StatementBodies : INotifyPropertyChanged
    {
        private string? name;

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
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
