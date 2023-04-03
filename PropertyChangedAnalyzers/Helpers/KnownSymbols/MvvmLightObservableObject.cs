namespace PropertyChangedAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class MvvmLightObservableObject : QualifiedType
{
    internal readonly QualifiedMethod Set;
    internal readonly QualifiedMethod RaisePropertyChanged;
    internal readonly QualifiedMethod RaisePropertyChangedOfT;

    internal MvvmLightObservableObject()
        : base("GalaSoft.MvvmLight.ObservableObject")
    {
        this.Set = new QualifiedMethod(this, nameof(this.Set));
        this.RaisePropertyChanged = new QualifiedMethod(this, nameof(this.RaisePropertyChanged));
        this.RaisePropertyChangedOfT = new QualifiedMethod(this, $"{nameof(this.RaisePropertyChanged)}`1");
    }
}
