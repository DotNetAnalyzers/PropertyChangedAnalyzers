namespace ValidCode.Recursion
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class NotRecursion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T location, T value, [CallerMemberName] string? propertyName = null)
        {
            if (RuntimeHelpers.Equals(location, value)) return false;
            location = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        record DifferentClass
        {
            public int P;
        }

        private int p;
        public int P
        {
            get => p;
            set
            {
                if (!Set(ref p, value)) return;

                _ = new DifferentClass { P = value };
                _ = new DifferentClass() with { P = value };

                {
                    int P;
                    P = value;
                }

                _ = new Action<int>(P =>
                {
                    P = value;
                });

                LocalFunction(42);
                void LocalFunction(int P)
                {
                    P = value;
                }
            }
        }
    }
}
