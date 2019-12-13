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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC007MissingInvoker,
            Descriptors.INPC011DoNotShadow);

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
                if (Shadows())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC011DoNotShadow, context.Node.GetLocation()));
                }

                if (MissingInvoker())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC007MissingInvoker, context.Node.GetLocation()));
                }
            }

            bool Shadows()
            {
                return eventSymbol is { IsStatic: false, IsOverride: false } &&
                       eventSymbol.ContainingType.BaseType.TryFindEventRecursive(eventSymbol.Name, out _);
            }

            bool MissingInvoker()
            {
                if (eventSymbol is { IsStatic: false, ContainingType: { TypeKind: TypeKind.Class } } &&
                    eventSymbol == KnownSymbol.INotifyPropertyChanged.PropertyChanged &&
                    !OnPropertyChanged.TryFind(eventSymbol, context.SemanticModel, context.CancellationToken, out _))
                {
                    return !eventSymbol.ContainingType.IsSealed ||
                           eventSymbol.ContainingType.GetMembers().TryFirstOfType(x => x.SetMethod != null, out IPropertySymbol _);
                }

                return eventSymbol.IsStatic &&
                       !OnPropertyChanged.TryFind(eventSymbol, context.SemanticModel, context.CancellationToken, out _);
            }
        }
    }
}
