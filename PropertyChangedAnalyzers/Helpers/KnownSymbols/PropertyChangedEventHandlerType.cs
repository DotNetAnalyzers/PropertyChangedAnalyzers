namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class PropertyChangedEventHandlerType : QualifiedType
    {
        internal readonly QualifiedMethod Invoke;

        internal PropertyChangedEventHandlerType()
            : base("System.ComponentModel.PropertyChangedEventHandler")
        {
            this.Invoke = new QualifiedMethod(this, nameof(this.Invoke));
        }
    }
}
