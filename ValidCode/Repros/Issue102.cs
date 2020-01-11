namespace ValidCode.Repros
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public interface IIssue102
    {
        INotifyPropertyChanged Source { get; set; }
    }

    public sealed class Issue102<T> : INotifyPropertyChanged, IDisposable, IIssue102
         where T : class, INotifyPropertyChanged
    {
        private readonly PropertyChangedEventHandler? onTrackedPropertyChanged = null;
        private readonly object gate = new object();

        private T source;
        private bool disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public T Source
        {
            get => this.source;

            set
            {
                if (this.disposed)
                {
                    return;
                }

                lock (this.gate)
                {
                    if (this.disposed ||
#pragma warning disable INPC006_b
                        ReferenceEquals(value, this.source))
#pragma warning restore INPC006_b
                    {
                        return;
                    }

                    if (this.source != null)
                    {
                        this.source.PropertyChanged -= this.onTrackedPropertyChanged;
                    }

                    if (value != null)
                    {
                        value.PropertyChanged += this.onTrackedPropertyChanged;
                    }

                    this.source = value;
                    this.OnPropertyChanged();
                }
            }
        }

        INotifyPropertyChanged IIssue102.Source
        {
            get => this.source;
            set => this.Source = (T)value;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            INotifyPropertyChanged oldSource;
            lock (this.gate)
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;
                oldSource = this.source;
                this.source = null;
            }

            if (oldSource != null)
            {
                oldSource.PropertyChanged -= this.onTrackedPropertyChanged;
            }
        }
    }
}
