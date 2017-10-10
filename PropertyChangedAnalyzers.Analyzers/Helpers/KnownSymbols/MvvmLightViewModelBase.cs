namespace PropertyChangedAnalyzers
{
    internal class MvvmLightViewModelBase : QualifiedType
    {
        internal readonly QualifiedMethod Set;
        internal readonly QualifiedMethod RaisePropertyChanged;

        internal MvvmLightViewModelBase()
            : base("GalaSoft.MvvmLight.ViewModelBase")
        {
            this.Set = new QualifiedMethod(this, nameof(this.Set));
            this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
        }
    }
}