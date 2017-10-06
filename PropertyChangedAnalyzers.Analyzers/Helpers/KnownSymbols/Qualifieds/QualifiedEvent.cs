namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal class QualifiedEvent : QualifiedMember<IEventSymbol>
    {
        public QualifiedEvent(QualifiedType containingType, string name)
            : base(containingType, name)
        {
        }
    }
}