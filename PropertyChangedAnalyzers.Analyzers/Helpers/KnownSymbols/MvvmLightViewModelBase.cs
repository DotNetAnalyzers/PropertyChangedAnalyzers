namespace PropertyChangedAnalyzers
{
    internal class MvvmLightViewModelBase : QualifiedType
    {
        internal readonly QualifiedMethod Set;

        internal MvvmLightViewModelBase()
            : base("GalaSoft.MvvmLight.ViewModelBase")
        {
            this.Set = new QualifiedMethod(this, nameof(this.Set));
        }
    }
}