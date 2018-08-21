// ReSharper disable All
namespace ValidCode.Inheritance
{
    public sealed class StatementBodies : StatementBodiesViewModelBase
    {
        private string name;

        public string Greeting
        {
            get
            {
                return $"Hello {this.name}";
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }

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
    }
}
