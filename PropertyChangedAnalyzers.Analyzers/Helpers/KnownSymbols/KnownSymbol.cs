// ReSharper disable InconsistentNaming
namespace PropertyChangedAnalyzers
{
    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new ObjectType();
        internal static readonly StringType String = new StringType();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");
        internal static readonly QualifiedType Attribute = Create("System.Attribute");
        internal static readonly NullableType Nullable = new NullableType();
        internal static readonly NullableOfTType NullableOfT = new NullableOfTType();
        internal static readonly EqualityComparerOfTType EqualityComparerOfT = new EqualityComparerOfTType();
        internal static readonly QualifiedType LambdaExpression = Create("System.Linq.Expressions.LambdaExpression");
        internal static readonly QualifiedType LinqExpressionOfT = Create("System.Linq.Expressions.Expression`1");

        internal static readonly QualifiedType CallerMemberNameAttribute = new QualifiedType("System.Runtime.CompilerServices.CallerMemberNameAttribute");
        internal static readonly INotifyPropertyChangedType INotifyPropertyChanged = new INotifyPropertyChangedType();
        internal static readonly QualifiedType PropertyChangedEventArgs = new QualifiedType("System.ComponentModel.PropertyChangedEventArgs");
        internal static readonly PropertyChangedEventHandlerType PropertyChangedEventHandler = new PropertyChangedEventHandlerType();

        internal static readonly DependencyObjectType DependencyObject = new DependencyObjectType();
        internal static readonly FrameworkElementType FrameworkElement = new FrameworkElementType();
        internal static readonly DependencyPropertyType DependencyProperty = new DependencyPropertyType();
        internal static readonly DependencyPropertyKeyType DependencyPropertyKey = new DependencyPropertyKeyType();
        internal static readonly QualifiedType PropertyMetadata = Create("System.Windows.PropertyMetadata");
        internal static readonly QualifiedType DependencyPropertyChangedEventArgs = Create("System.Windows.DependencyPropertyChangedEventArgs");

        internal static readonly QualifiedType PropertyChangedCallback = Create("System.Windows.PropertyChangedCallback");
        internal static readonly QualifiedType CoerceValueCallback = Create("System.Windows.CoerceValueCallback");
        internal static readonly QualifiedType ValidateValueCallback = Create("System.Windows.ValidateValueCallback");

        internal static readonly QualifiedType FontFamily = Create("System.Windows.Media.FontFamily");
        internal static readonly QualifiedType Freezable = Create("System.Windows.Freezable");
        internal static readonly QualifiedType DataTemplateSelector = Create("System.Windows.Controls.DataTemplateSelector");
        internal static readonly QualifiedType MarkupExtension = Create("System.Windows.Markup.MarkupExtension");
        internal static readonly QualifiedType IValueConverter = Create("System.Windows.Data.IValueConverter");
        internal static readonly QualifiedType IMultiValueConverter = Create("System.Windows.Data.IMultiValueConverter");

        internal static readonly XmlnsPrefixAttributeType XmlnsPrefixAttribute = new XmlnsPrefixAttributeType();
        internal static readonly XmlnsDefinitionAttributeType XmlnsDefinitionAttribute = new XmlnsDefinitionAttributeType();

        internal static readonly MvvmLightViewModelBase MvvmLightViewModelBase = new MvvmLightViewModelBase();
        internal static readonly MvvmLightObservableObject MvvmLightObservableObject = new MvvmLightObservableObject();
        internal static readonly CaliburnMicroPropertyChangedBase CaliburnMicroPropertyChangedBase = new CaliburnMicroPropertyChangedBase();
        internal static readonly StyletPropertyChangedBase StyletPropertyChangedBase = new StyletPropertyChangedBase();
        internal static readonly MvvmCrossMvxNotifyPropertyChanged MvvmCrossMvxNotifyPropertyChanged = new MvvmCrossMvxNotifyPropertyChanged();
        internal static readonly MvvmCrossCoreMvxNotifyPropertyChanged MvvmCrossCoreMvxNotifyPropertyChanged = new MvvmCrossCoreMvxNotifyPropertyChanged();
        internal static readonly MicrosoftPracticesPrismMvvmBindableBase MicrosoftPracticesPrismMvvmBindableBase = new MicrosoftPracticesPrismMvvmBindableBase();

        private static QualifiedType Create(string qualifiedName, string alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
