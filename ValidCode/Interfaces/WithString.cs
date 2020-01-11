namespace ValidCode.Interfaces
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class WithString : IValue, INotifyPropertyChanged
    {
        private string? value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Value
        {
            get => this.value;
            set
            {
                if (value == this.value)
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
            set => this.Value = (string?)value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
