namespace PropertyChangedAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class StyletPropertyChangedBase : QualifiedType
{
    internal readonly QualifiedMethod SetAndNotify;
    internal readonly QualifiedMethod NotifyOfPropertyChange;
    internal readonly QualifiedMethod NotifyOfPropertyChangeOfT;

    internal StyletPropertyChangedBase()
        : base("Stylet.PropertyChangedBase")
    {
        this.SetAndNotify = new QualifiedMethod(this, nameof(this.SetAndNotify));
        this.NotifyOfPropertyChange = new QualifiedMethod(this, nameof(this.NotifyOfPropertyChange));
        this.NotifyOfPropertyChangeOfT = new QualifiedMethod(this, $"{nameof(this.NotifyOfPropertyChange)}`1");
    }
}
