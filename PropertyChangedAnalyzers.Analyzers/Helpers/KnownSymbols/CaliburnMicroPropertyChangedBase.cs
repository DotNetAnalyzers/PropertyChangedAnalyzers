namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class CaliburnMicroPropertyChangedBase : QualifiedType
    {
        internal readonly QualifiedMethod Set;
        internal readonly QualifiedMethod NotifyOfPropertyChange;
        internal readonly QualifiedMethod NotifyOfPropertyChangeOfT;

        internal CaliburnMicroPropertyChangedBase()
            : base("Caliburn.Micro.PropertyChangedBase")
        {
            this.Set = new QualifiedMethod(this, nameof(this.Set));
            this.NotifyOfPropertyChange = new QualifiedMethod(this, nameof(this.NotifyOfPropertyChange));
            this.NotifyOfPropertyChangeOfT = new QualifiedMethod(this, $"{nameof(this.NotifyOfPropertyChange)}`1");
        }
    }
}
