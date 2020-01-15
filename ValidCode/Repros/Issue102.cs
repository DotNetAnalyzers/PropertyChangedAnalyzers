// ReSharper disable All
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

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private T source;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
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

#pragma warning disable CS8601 // Possible null reference assignment.
                    this.source = value;
#pragma warning restore CS8601 // Possible null reference assignment.
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                this.source = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

            if (oldSource != null)
            {
                oldSource.PropertyChanged -= this.onTrackedPropertyChanged;
            }
        }
    }
}
