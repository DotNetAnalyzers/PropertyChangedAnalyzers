namespace ValidCode.Interfaces
{
    public class WithString : IValue
    {
        private string value;

        public string Value
        {
            get => this.value;
            set => this.value = value;
        }

        object IValue.Value
        {
            get => this.value;
            set => this.Value = (string)value;
        }
    }
}
