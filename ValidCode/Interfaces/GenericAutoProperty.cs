namespace ValidCode.Interfaces
{
    public class GenericAutoProperty<T> : IValue
    {
        public T Value { get; set; }

        object IValue.Value
        {
            get => this.Value;
            set => this.Value = (T)value;
        }
    }
}