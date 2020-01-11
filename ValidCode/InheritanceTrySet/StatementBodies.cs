// ReSharper disable All
#pragma warning disable INPC020 // Prefer expression body accessor.
namespace ValidCode.InheritanceTrySet
{
    public sealed class StatementBodies : StatementBodiesViewModelBase
    {
        private string? name;
        private int value;

        public string Greeting => $"Hello {this.name}";

        public string? Name
        {
            get => this.name;

            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.TrySet(ref this.value, value);
            }
        }
    }
}
