namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class Descriptors
    {
        internal static readonly DiagnosticDescriptor INPC001ImplementINotifyPropertyChanged = Descriptors.Create(
            id: "INPC001",
            title: "The class has mutable properties and should implement INotifyPropertyChanged.",
            messageFormat: "{0}",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The class has mutable properties and should implement INotifyPropertyChanged.");

        internal static readonly DiagnosticDescriptor INPC002MutablePublicPropertyShouldNotify = Descriptors.Create(
            id: "INPC002",
            title: "Mutable public property should notify.",
            messageFormat: "Property '{0}' should notify when value changes.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "All mutable public properties should notify when their value changes.");

        internal static readonly DiagnosticDescriptor INPC003NotifyForDependentProperty = Descriptors.Create(
            id: "INPC003",
            title: "Notify when property changes.",
            messageFormat: "Notify that property '{0}' changes.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Notify when property changes.");

        internal static readonly DiagnosticDescriptor INPC004UseCallerMemberName = Descriptors.Create(
            id: "INPC004",
            title: "Use [CallerMemberName]",
            messageFormat: "Use [CallerMemberName]",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use [CallerMemberName]");

        internal static readonly DiagnosticDescriptor INPC005CheckIfDifferentBeforeNotifying = Descriptors.Create(
            id: "INPC005",
            title: "Check if value is different before notifying.",
            messageFormat: "Check if value is different before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Check if value is different before notifying.");

        internal static readonly DiagnosticDescriptor INPC006UseObjectEqualsForReferenceTypes = Descriptors.Create(
            id: "INPC006_b",
            title: "Check if value is different using object.Equals before notifying.",
            messageFormat: "Check if value is different using object.Equals before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: false,
            description: "Check if value is different using object.Equals before notifying.");

        internal static readonly DiagnosticDescriptor INPC006UseReferenceEqualsForReferenceTypes = Descriptors.Create(
            id: "INPC006_a",
            title: "Check if value is different using ReferenceEquals before notifying.",
            messageFormat: "Check if value is different using ReferenceEquals before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "Check if value is different using ReferenceEquals before notifying.");

        internal static readonly DiagnosticDescriptor INPC007MissingInvoker = Descriptors.Create(
            id: "INPC007",
            title: "The class has PropertyChangedEvent but no invoker.",
            messageFormat: "The class has PropertyChangedEvent but no invoker.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The class has PropertyChangedEvent but no invoker.");

        internal static readonly DiagnosticDescriptor INPC008StructMustNotNotify = Descriptors.Create(
            id: "INPC008",
            title: "Struct must not implement INotifyPropertyChanged",
            messageFormat: "Struct '{0}' implements INotifyPropertyChanged",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Struct must not implement INotifyPropertyChanged");

        internal static readonly DiagnosticDescriptor INPC009DoNotRaiseChangeForMissingProperty = Descriptors.Create(
            id: "INPC009",
            title: "Don't raise PropertyChanged for missing property.",
            messageFormat: "Don't raise PropertyChanged for missing property.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't raise PropertyChanged for missing property.");

        internal static readonly DiagnosticDescriptor INPC010GetAndSetSame = Descriptors.Create(
            id: "INPC010",
            title: "The property sets a different field than it returns.",
            messageFormat: "The property sets a different field than it returns.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The property sets a different field than it returns.");

        internal static readonly DiagnosticDescriptor INPC011DoNotShadow = Descriptors.Create(
            id: "INPC011",
            title: "Don't shadow PropertyChanged event.",
            messageFormat: "Don't shadow PropertyChanged event.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Don't shadow PropertyChanged event.");

        internal static readonly DiagnosticDescriptor INPC012DoNotUseExpression = Descriptors.Create(
            id: "INPC012",
            title: "Don't use expression for raising PropertyChanged.",
            messageFormat: "Don't use expression for raising PropertyChanged.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Don't use expression for raising PropertyChanged.");

        internal static readonly DiagnosticDescriptor INPC013UseNameof = Descriptors.Create(
            id: "INPC013",
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use nameof.");

        internal static readonly DiagnosticDescriptor INPC014SetBackingFieldInConstructor = Descriptors.Create(
            id: "INPC014",
            title: "Prefer setting backing field in constructor.",
            messageFormat: "Prefer setting backing field in constructor.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Prefer setting backing field in constructor.");

        internal static readonly DiagnosticDescriptor INPC015PropertyIsRecursive = Descriptors.Create(
            id: "INPC015",
            title: "Property is recursive.",
            messageFormat: "Property is recursive {0}.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Property is recursive.");

        internal static readonly DiagnosticDescriptor INPC016NotifyAfterMutation = Descriptors.Create(
            id: "INPC016",
            title: "Notify after update.",
            messageFormat: "Notify after updating the backing field.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Notify after updating the backing field.");

        internal static readonly DiagnosticDescriptor INPC017BackingFieldNameMisMatch = Descriptors.Create(
            id: "INPC017",
            title: "Backing field name must match.",
            messageFormat: "Backing field name must match.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing field name must match.");

        internal static readonly DiagnosticDescriptor INPC018InvokerShouldBeProtected = Descriptors.Create(
            id: "INPC018",
            title: "PropertyChanged invoker should be protected when the class is not sealed.",
            messageFormat: "PropertyChanged invoker should be protected when the class is not sealed.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "PropertyChanged invoker should be protected when the class is not sealed.");

        internal static readonly DiagnosticDescriptor INPC019GetBackingField = Create(
            id: "INPC019",
            title: "Getter should return backing field.",
            messageFormat: "Getter should return backing field.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Getter should return backing field.");

        internal static readonly DiagnosticDescriptor INPC020PreferExpressionBodyAccessor = Descriptors.Create(
            id: "INPC020",
            title: "Prefer expression body accessor.",
            messageFormat: "Prefer expression body accessor.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Prefer expression body accessor.");

        internal static readonly DiagnosticDescriptor INPC021SetBackingField = Descriptors.Create(
            id: "INPC021",
            title: "Setter should set backing field.",
            messageFormat: "Setter should set backing field.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Setter should set backing field.");

        /// <summary>
        /// Create a DiagnosticDescriptor, which provides description about a <see cref="Diagnostic" />.
        /// </summary>
        /// <param name="id">A unique identifier for the diagnostic. For example, code analysis diagnostic ID "CA1001".</param>
        /// <param name="title">A short title describing the diagnostic. For example, for CA1001: "Types that own disposable fields should be disposable".</param>
        /// <param name="messageFormat">A format message string, which can be passed as the first argument to <see cref="string.Format(string,object[])" /> when creating the diagnostic message with this descriptor.
        /// For example, for CA1001: "Implement IDisposable on '{0}' because it creates members of the following IDisposable types: '{1}'.</param>
        /// <param name="category">The category of the diagnostic (like Design, Naming etc.). For example, for CA1001: "Microsoft.Design".</param>
        /// <param name="defaultSeverity">Default severity of the diagnostic.</param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default.</param>
        /// <param name="description">An optional longer description of the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic. See <see cref="WellKnownDiagnosticTags" /> for some well known tags.</param>
        internal static DiagnosticDescriptor Create(
            string id,
            string title,
            string messageFormat,
            string category,
            DiagnosticSeverity defaultSeverity,
            bool isEnabledByDefault,
            string description,
            params string[] customTags)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: isEnabledByDefault,
                description: description,
                helpLinkUri: $"https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/{id}.md",
                customTags: customTags);
        }
    }
}
