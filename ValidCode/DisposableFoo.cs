namespace ValidCode
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class DisposableFoo : INotifyPropertyChanged, IDisposable
    {
        private bool disposed;
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                this.ThrowIfDisposed();
                return this.name;
            }

            set
            {
                this.ThrowIfDisposed();
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
