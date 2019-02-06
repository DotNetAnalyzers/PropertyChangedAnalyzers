// ReSharper disable All
namespace ValidCode
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class RelayProperty : INotifyPropertyChanged
    {
        private int p1;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P2));
                this.OnPropertyChanged(nameof(this.P3));
            }
        }

        public int P2
        {
            get => this.P1;
            set => this.P1 = value;
        }

        public int P3
        {
#pragma warning disable INPC017 // INPC017 Backing field name must match.
            get => this.p1;
#pragma warning restore INPC017
            set => this.P1 = value;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
