namespace PropertyChangedAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class MvvmLightViewModelBase : QualifiedType
{
    internal readonly QualifiedMethod Set;
    internal readonly QualifiedMethod RaisePropertyChanged;
    internal readonly QualifiedMethod RaisePropertyChangedOfT;

    internal MvvmLightViewModelBase()
        : base("GalaSoft.MvvmLight.ViewModelBase")
    {
        this.Set = new QualifiedMethod(this, nameof(this.Set));
        this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
        this.RaisePropertyChangedOfT = new QualifiedMethod(this, $"{nameof(this.RaisePropertyChanged)}`1");
    }
}
