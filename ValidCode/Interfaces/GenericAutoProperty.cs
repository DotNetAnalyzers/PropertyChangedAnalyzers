namespace ValidCode.Interfaces
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class GenericAutoProperty<T> : IValue, INotifyPropertyChanged
    {
        private T value;

        public event PropertyChangedEventHandler PropertyChanged;

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

        object IValue.Value
        {
            get => this.Value;
            set => this.Value = (T)value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
