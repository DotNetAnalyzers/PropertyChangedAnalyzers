namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC007MissingInvoker : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC007";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The class has PropertyChangedEvent but no invoker.",
            messageFormat: "The class has PropertyChangedEvent but no invoker.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "The class has PropertyChangedEvent but no invoker.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(HandleEventField, SyntaxKind.EventFieldDeclaration);
        }

        private static void HandleEventField(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is EventFieldDeclarationSyntax eventFieldDeclaration &&
                context.ContainingSymbol is IEventSymbol eventSymbol &&
                 !eventSymbol.ContainingType.IsInterface())
            {
                if (eventSymbol == KnownSymbol.INotifyPropertyChanged.PropertyChanged)
                {
                    if (!PropertyChanged.TryGetInvoker(eventSymbol, context.SemanticModel, context.CancellationToken, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, eventFieldDeclaration.GetLocation()));
                    }
                }

                if (eventSymbol.ContainingType.IsStatic &&
                    eventSymbol.Type == KnownSymbol.PropertyChangedEventHandler &&
                    eventSymbol.Name == "PropertyChanged")
                {
                    if (!PropertyChanged.TryGetInvoker(eventSymbol, context.SemanticModel, context.CancellationToken, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, eventFieldDeclaration.GetLocation()));
                    }
                }
            }
        }
    }
}