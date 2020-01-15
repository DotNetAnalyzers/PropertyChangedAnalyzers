namespace ValidCode.Wrapping
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class FrameworkTypes : INotifyPropertyChanged
    {
        private TimeSpan timeSpan1;
        private TimeSpan timeSpan2;
        private Point point1;
        private Point point2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public long Ticks
        {
            get => this.timeSpan1.Ticks;
            set
            {
                if (value == this.timeSpan1.Ticks)
                {
                    return;
                }

                this.timeSpan1 = TimeSpan.FromTicks(value);
                this.OnPropertyChanged();
            }
        }

        public long TicksTrySet
        {
            get => this.timeSpan2.Ticks;
            set => this.TrySet(ref this.timeSpan2, TimeSpan.FromTicks(value));
        }


        public int X1
        {
            get => this.point1.X;
            set
            {
                if (value == this.point1.X)
                {
                    return;
                }

#pragma warning disable INPC003 // Notify when property changes.
                this.point1 = new Point(value, this.Y1);
#pragma warning restore INPC003 // Notify when property changes.
                this.OnPropertyChanged();
            }
        }

        public int Y1
        {
            get => this.point1.Y;
            set
            {
                if (value == this.point1.Y)
                {
                    return;
                }

#pragma warning disable INPC003 // Notify when property changes.
                this.point1 = new Point(this.point1.X, value);
#pragma warning restore INPC003 // Notify when property changes.
                this.OnPropertyChanged();
            }
        }

        public int X2
        {
            get => this.point2.X;
#pragma warning disable INPC003 // Notify when property changes.
            set => this.TrySet(ref this.point2, new Point(value, this.point2.Y));
#pragma warning restore INPC003 // Notify when property changes.
        }

        public int Y2
        {
            get => this.point2.Y;
#pragma warning disable INPC003 // Notify when property changes.
            set => this.TrySet(ref this.point2, new Point(value, this.point2.Y));
#pragma warning restore INPC003 // Notify when property changes.
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
