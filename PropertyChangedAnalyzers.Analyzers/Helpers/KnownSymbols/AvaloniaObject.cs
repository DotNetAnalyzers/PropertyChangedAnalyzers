namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class AvaloniaObject : QualifiedType
    {
        internal readonly QualifiedMethod SetAndRaiseT;
        internal readonly QualifiedMethod RaisePropertyChanged;

        internal AvaloniaObject()
        : base("Avalonia.AvaloniaObject")
        {
            this.SetAndRaiseT = new QualifiedMethod(this, $"{nameof(this.SetAndRaiseT)}`1");
            this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
        }
    }
}
