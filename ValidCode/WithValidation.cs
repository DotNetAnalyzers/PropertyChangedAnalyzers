namespace ValidCode
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class WithValidation : INotifyPropertyChanged
    {
        private int p1;
        private int p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public WithValidation(int p1, int p2)
        {
            this.P1 = p1;
            this.P2 = p2;
        }

        public int P1
        {
            get => this.p1;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Expected greater or equal to zero.");
                }

                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

        public int P2
        {
            get => this.p2;
            set
            {
                if (value < this.p1)
                {
                    throw new ArgumentException("Expected greater or equal to zero.");
                }

                this.TrySet(ref this.p2, value);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
