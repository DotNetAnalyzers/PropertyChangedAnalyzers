// ReSharper disable InconsistentNaming
namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new();
        internal static readonly StringType String = new();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");
        internal static readonly QualifiedType Attribute = Create("System.Attribute");
        internal static readonly QualifiedType IEnumerator = Create("System.Collections.IEnumerator");
        internal static readonly QualifiedType Stream = Create("System.IO.Stream");
        internal static readonly NullableType Nullable = new();
        internal static readonly NullableOfTType NullableOfT = new();
        internal static readonly EqualityComparerOfTType EqualityComparerOfT = new();
        internal static readonly QualifiedType LinqExpressionOfT = Create("System.Linq.Expressions.Expression`1");
        internal static readonly QualifiedType ConcurrentDictionaryOfTKeyTValue = Create("System.Collections.Concurrent.ConcurrentDictionary`2");

        internal static readonly QualifiedType CallerMemberNameAttribute = new("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        internal static readonly INotifyPropertyChangedType INotifyPropertyChanged = new();
        internal static readonly QualifiedType PropertyChangedEventArgs = new("System.ComponentModel.PropertyChangedEventArgs");
        internal static readonly QualifiedType BindableAttribute = new("System.ComponentModel.BindableAttribute");
        internal static readonly PropertyChangedEventHandlerType PropertyChangedEventHandler = new();

        internal static readonly DependencyObjectType DependencyObject = new();
        internal static readonly QualifiedType DataTemplateSelector = Create("System.Windows.Controls.DataTemplateSelector");
        internal static readonly QualifiedType MarkupExtension = Create("System.Windows.Markup.MarkupExtension");
        internal static readonly QualifiedType IValueConverter = Create("System.Windows.Data.IValueConverter");
        internal static readonly QualifiedType IMultiValueConverter = Create("System.Windows.Data.IMultiValueConverter");

        internal static readonly MvvmLightViewModelBase MvvmLightViewModelBase = new();
        internal static readonly MvvmLightObservableObject MvvmLightObservableObject = new();
        internal static readonly CaliburnMicroPropertyChangedBase CaliburnMicroPropertyChangedBase = new();
        internal static readonly StyletPropertyChangedBase StyletPropertyChangedBase = new();
        internal static readonly MvvmCrossMvxNotifyPropertyChanged MvvmCrossMvxNotifyPropertyChanged = new();
        internal static readonly MvvmCrossCoreMvxNotifyPropertyChanged MvvmCrossCoreMvxNotifyPropertyChanged = new();
        internal static readonly MicrosoftPracticesPrismMvvmBindableBase MicrosoftPracticesPrismMvvmBindableBase = new();

        internal static readonly QualifiedType JetbrainsNotifyPropertyChangedInvocatorAttribute = new("Jetbrains.Annotations.NotifyPropertyChangedInvocatorAttribute");

        private static QualifiedType Create(string qualifiedName, string? alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
