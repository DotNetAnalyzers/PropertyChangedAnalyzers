namespace PropertyChangedAnalyzers
{
    internal class CaliburnMicroPropertyChangedBase : QualifiedType
    {
        internal readonly QualifiedMethod Set;
        internal readonly QualifiedMethod NotifyOfPropertyChange;

        internal CaliburnMicroPropertyChangedBase()
            : base("Caliburn.Micro.PropertyChangedBase")
        {
            this.Set = new QualifiedMethod(this, nameof(this.Set));
            this.NotifyOfPropertyChange = new QualifiedMethod(this, nameof(this.NotifyOfPropertyChange));
        }
    }
}