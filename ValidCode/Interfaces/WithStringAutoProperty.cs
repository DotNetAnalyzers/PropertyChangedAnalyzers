namespace ValidCode.Interfaces
{
#pragma warning disable INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
    public class WithStringAutoProperty : IValue
#pragma warning restore INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
    {
        public string Value { get; set; }

        object IValue.Value
        {
            get => this.Value;
            set => this.Value = (string)value;
        }
    }
}
