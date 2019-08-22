namespace ValidCode.Interfaces
{
#pragma warning disable INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
    public class GenericAutoProperty<T> : IValue
#pragma warning restore INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
    {
        public T Value { get; set; }

        object IValue.Value
        {
            get => this.Value;
            set => this.Value = (T)value;
        }
    }
}
