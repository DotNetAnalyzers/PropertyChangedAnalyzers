namespace ValidCode.Interfaces
{
    public class Generic<T> : IValue
    {
        private T value;

        public T Value
        {
            get => this.value;
            set => this.value = value;
        }

        object IValue.Value
        {
            get => this.value;
            set => this.Value = (T)value;
        }
    }
}
