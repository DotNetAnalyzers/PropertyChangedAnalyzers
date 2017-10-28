namespace PropertyChangedAnalyzers
{
    internal class MvvmCrossCoreMvxMvxNotifyPropertyChanged : QualifiedType
    {
        internal readonly QualifiedMethod SetProperty;
        internal readonly QualifiedMethod RaisePropertyChanged;
        internal readonly QualifiedMethod RaisePropertyChangedOfT;

        internal MvvmCrossCoreMvxMvxNotifyPropertyChanged()
            : base("MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged")
        {
            this.SetProperty = new QualifiedMethod(this, nameof(this.SetProperty));
            this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
            this.RaisePropertyChangedOfT = new QualifiedMethod(this, $"{nameof(this.RaisePropertyChanged)}`1");
        }
    }
}