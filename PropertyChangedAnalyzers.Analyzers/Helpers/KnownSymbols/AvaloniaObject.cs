namespace PropertyChangedAnalyzers
{
    internal class AvaloniaObject : QualifiedType
    {
        internal readonly QualifiedMethod SetAndRaise;

        internal AvaloniaObject()
            : base("Avalonia.AvaloniaObject")
        {
            this.SetAndRaise = new QualifiedMethod(this, nameof(this.SetAndRaise));
        }
    }
}