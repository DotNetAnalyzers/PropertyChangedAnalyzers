namespace PropertyChangedAnalyzers
{
    internal class CaliburnMicroPropertyChangedBase : QualifiedType
    {
        internal readonly QualifiedMethod Set;

        internal CaliburnMicroPropertyChangedBase()
            : base("Caliburn.Micro.PropertyChangedBase")
        {
            this.Set = new QualifiedMethod(this, nameof(this.Set));
        }
    }
}