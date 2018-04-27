namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class FrameworkElementType : QualifiedType
    {
        internal readonly QualifiedProperty DataContext;
        internal readonly QualifiedField DataContextProperty;

        internal FrameworkElementType()
            : base("System.Windows.FrameworkElement")
        {
            this.DataContext = new QualifiedProperty(this, nameof(this.DataContext));
            this.DataContextProperty = new QualifiedField(this, nameof(this.DataContextProperty));
        }
    }
}
