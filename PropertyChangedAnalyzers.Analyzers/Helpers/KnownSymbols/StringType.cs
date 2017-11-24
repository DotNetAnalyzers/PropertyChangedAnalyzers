namespace PropertyChangedAnalyzers
{
    internal class StringType : QualifiedType
    {
        internal new readonly QualifiedMethod Equals;

        internal StringType()
            : base("System.String")
        {
            this.Equals = new QualifiedMethod(this, nameof(this.Equals));
        }
    }
}