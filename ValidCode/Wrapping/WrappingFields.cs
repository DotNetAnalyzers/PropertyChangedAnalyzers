namespace ValidCode.Wrapping
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class WrappingFields : INotifyPropertyChanged
    {
        private readonly WithFields withFields = new WithFields();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1
        {
            get => this.withFields.F1;
            set
            {
                if (value == this.withFields.F1)
                {
                    return;
                }

                this.withFields.F1 = value;
                this.OnPropertyChanged();
            }
        }

        public int P2
        {
            get => this.withFields.F2;
            set => this.TrySet(ref this.withFields.F2, value);
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
    }
}
