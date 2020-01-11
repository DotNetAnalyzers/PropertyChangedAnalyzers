namespace ValidCode
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class CachingInConcurrentDictionary : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, Cache.GetOrAdd(propertyName, name => new PropertyChangedEventArgs(name)));
        }
    }
}