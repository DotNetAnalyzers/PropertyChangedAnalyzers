namespace PropertyChangedAnalyzers
{
    internal class IReactiveObject : QualifiedType
    {
        internal QualifiedMethod SetAndRaiseIfChanged;
        internal QualifiedMethod RaisePropertyChanged;

        internal IReactiveObject()
            : base("ReactiveUI.IReactiveObject")
        {
            this.SetAndRaiseIfChanged = new QualifiedMethod(this, nameof(this.SetAndRaiseIfChanged));
            this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
        }
    }
}
