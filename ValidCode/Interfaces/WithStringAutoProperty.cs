namespace ValidCode.Interfaces
{
    public class WithStringAutoProperty : IValue
    {
        public string Value { get; set; }

        object IValue.Value
        {
            get => this.Value;
            set => this.Value = (string)value;
        }
    }
}