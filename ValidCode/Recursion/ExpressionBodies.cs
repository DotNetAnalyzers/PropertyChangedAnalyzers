// ReSharper disable All
#pragma warning disable INPC007, INPC015, CS0067
namespace ValidCode.Recursion
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ExpressionBodies : INotifyPropertyChanged
    {
        private string name;
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => this.Greeting;

        public string Name
        {
            get => this.Name;

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
            get => this.Value;
            set => this.TrySet(ref this.value, value);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.OnPropertyChanged(propertyName);

        private bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) => this.TrySet(ref field, newValue, propertyName);
    }
}
