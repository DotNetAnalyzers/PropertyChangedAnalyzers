namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC007MissingInvoker : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.INPC007MissingInvoker);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.EventFieldDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is IEventSymbol eventSymbol &&
                eventSymbol.Type == KnownSymbol.PropertyChangedEventHandler &&
                eventSymbol.Name == "PropertyChanged")
            {
                if (!eventSymbol.IsStatic &&
                    eventSymbol.ContainingType.TypeKind != TypeKind.Interface &&
                    eventSymbol == KnownSymbol.INotifyPropertyChanged.PropertyChanged &&
                    !PropertyChanged.TryGetOnPropertyChanged(eventSymbol, context.SemanticModel, context.CancellationToken, out _))
                {
                    if (eventSymbol.ContainingType.IsSealed &&
                        !eventSymbol.ContainingType.GetMembers().TryFirstOfType(x => x.SetMethod != null, out IPropertySymbol _))
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC007MissingInvoker, context.Node.GetLocation()));
                }
                else if (eventSymbol.IsStatic &&
                         !PropertyChanged.TryGetOnPropertyChanged(eventSymbol, context.SemanticModel, context.CancellationToken, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC007MissingInvoker, context.Node.GetLocation()));
                }
            }
        }
    }
}
