namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class MvvmCrossCoreMvxNotifyPropertyChanged : QualifiedType
    {
        internal readonly QualifiedMethod SetProperty;
        internal readonly QualifiedMethod RaisePropertyChanged;
        internal readonly QualifiedMethod RaisePropertyChangedOfT;

        internal MvvmCrossCoreMvxNotifyPropertyChanged()
            : base("MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged")
        {
            this.SetProperty = new QualifiedMethod(this, nameof(this.SetProperty));
            this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
            this.RaisePropertyChangedOfT = new QualifiedMethod(this, $"{nameof(this.RaisePropertyChanged)}`1");
        }
    }
}
