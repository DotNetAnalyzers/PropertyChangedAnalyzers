namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class EventAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC007MissingInvoker,
            Descriptors.INPC011DoNotShadow);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.EventFieldDeclaration, SyntaxKind.EventDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is IEventSymbol eventSymbol &&
                eventSymbol.Type == KnownSymbol.PropertyChangedEventHandler)
            {
                if (!eventSymbol.IsOverride &&
                    !eventSymbol.IsStatic &&
                    eventSymbol.ContainingType.BaseType.TryFindEventRecursive(eventSymbol.Name, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC011DoNotShadow, context.Node.GetLocation()));
                }

                if (MissingInvoker())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC007MissingInvoker, context.Node.GetLocation()));
                }
            }

            bool MissingInvoker()
            {
                if (!eventSymbol.IsStatic &&
                    eventSymbol.ContainingType.TypeKind != TypeKind.Interface &&
                    eventSymbol == KnownSymbol.INotifyPropertyChanged.PropertyChanged &&
                    !PropertyChanged.TryGetOnPropertyChanged(eventSymbol, context.SemanticModel, context.CancellationToken, out _))
                {
                    return !eventSymbol.ContainingType.IsSealed ||
                           eventSymbol.ContainingType.GetMembers().TryFirstOfType(x => x.SetMethod != null, out IPropertySymbol _);
                }

                return eventSymbol.IsStatic &&
                       !PropertyChanged.TryGetOnPropertyChanged(eventSymbol, context.SemanticModel, context.CancellationToken, out _);
            }
        }
    }
}
