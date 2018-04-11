namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class AvaloniaObject : QualifiedType
    {
        internal readonly QualifiedMethod SetAndRaise;
        internal readonly QualifiedMethod RaisePropertyChanged;

        internal AvaloniaObject()
        : base("Avalonia.AvaloniaObject")
        {
            this.SetAndRaise = new QualifiedMethod(this, nameof(this.SetAndRaise));
            this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
        }
    }
}
