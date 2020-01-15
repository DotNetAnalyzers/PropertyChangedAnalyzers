// ReSharper disable All
namespace ValidCode
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class LockInSetter : INotifyPropertyChanged
    {
        private readonly object _busyLock = new object();
        private bool _value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Value
        {
            get => this._value;
            private set
            {
                lock (this._busyLock)
                {
                    if (value && this._value)
                    {
                        throw new InvalidOperationException();
                    }

                    if (value == this._value)
                    {
                        return;
                    }

                    this._value = value;
                }

                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
