// ReSharper disable All
#pragma warning disable IDE1006 // Naming Styles
namespace ValidCode
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class WithEventDeclaration : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => this.propertyChanged += value;
            remove => this.propertyChanged -= value;
        }

        private event PropertyChangedEventHandler? propertyChanged;

        public string Name
        {
            get => this.name;

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
