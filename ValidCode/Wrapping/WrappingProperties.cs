namespace ValidCode.Wrapping
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class WrappingProperties : INotifyPropertyChanged
    {
        private WithProperties withProperties = new WithProperties();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => this.withProperties.P1;
            set
            {
                if (value == this.withProperties.P1)
                {
                    return;
                }

                this.withProperties.P1 = value;
                this.OnPropertyChanged();
            }
        }

        public int P2
        {
            get => this.withProperties.P2;
#pragma warning disable INPC003 // Notify when property changes.
            set => this.TrySet(ref this.withProperties, new WithProperties { P1 = this.P1, P2 = value });
#pragma warning restore INPC003 // Notify when property changes.
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        private class WithProperties
        {
            public int P1 { get; set; }

            public int P2 { get; set; }
        }
    }
}
