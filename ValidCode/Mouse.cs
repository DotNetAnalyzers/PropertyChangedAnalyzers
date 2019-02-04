// ReSharper disable All
namespace ValidCode
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class Mouse
    {
        public static Point Position
        {
            get
            {
                if (GetCursorPos(out var p))
                {
                    return p;
                }

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new Win32Exception();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }

            set
            {
                if (!SetCursorPos(value.X, value.Y))
                {
                    throw new Win32Exception();
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);
    }
}
