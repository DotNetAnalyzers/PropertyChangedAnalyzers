namespace ValidCode.Inheritance
{
    public sealed class ExpressionBodies : ExpressionBodiesViewModelBase
    {
        private string? name;

        public string Greeting => $"Hello {this.name}";

        public string? Name
        {
            get => this.name;

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
