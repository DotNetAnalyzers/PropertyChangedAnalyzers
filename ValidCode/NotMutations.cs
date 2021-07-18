namespace ValidCode
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class NotMutations : INotifyPropertyChanged
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

#pragma warning disable INPC001
        internal class Settings
#pragma warning restore INPC001
        {
            public static Settings Default { get; } = new();

            public int SamePropertyNameOnUnrelatedInstance { get; set; }
        }

        private int samePropertyNameOnUnrelatedInstance;
        public int SamePropertyNameOnUnrelatedInstance
        {
            get => this.samePropertyNameOnUnrelatedInstance;
            set
            {
                if (!Set(ref samePropertyNameOnUnrelatedInstance, value)) return;

                Settings.Default.SamePropertyNameOnUnrelatedInstance = value;
            }
        }
    }
}
