namespace ValidCode.Vanilla
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ExpressionBodies : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Greeting => $"Hello {this.name}";

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
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
