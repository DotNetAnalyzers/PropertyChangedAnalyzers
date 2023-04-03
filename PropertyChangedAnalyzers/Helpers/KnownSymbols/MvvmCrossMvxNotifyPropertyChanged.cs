namespace PropertyChangedAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class MvvmCrossMvxNotifyPropertyChanged : QualifiedType
{
    internal readonly QualifiedMethod SetProperty;
    internal readonly QualifiedMethod RaisePropertyChanged;
    internal readonly QualifiedMethod RaisePropertyChangedOfT;

    internal MvvmCrossMvxNotifyPropertyChanged()
        : base("MvvmCross.ViewModels.MvxNotifyPropertyChanged")
    {
        this.SetProperty = new QualifiedMethod(this, nameof(this.SetProperty));
        this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
        this.RaisePropertyChangedOfT = new QualifiedMethod(this, $"{nameof(this.RaisePropertyChanged)}`1");
    }
}
