namespace ValidCode
{
    using System.ComponentModel;

    public static class StaticClass
    {
        private static string name;
        private static int number;

        public static event PropertyChangedEventHandler? PropertyChanged;

        public static string Name
        {
            get => name;
            set
            {
                if (name == value)
                {
                    return;
                }

                name = value;
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public static int Number
        {
            get => number;
            set
            {
                if(value == number)
                {
                    return;
                }

                number = value;
                OnPropertyChanged();
            }
        }

        private static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
