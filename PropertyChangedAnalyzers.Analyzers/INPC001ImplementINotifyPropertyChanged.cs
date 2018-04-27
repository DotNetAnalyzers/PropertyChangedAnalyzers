namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC001ImplementINotifyPropertyChanged : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC001";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Implement INotifyPropertyChanged.",
            messageFormat: "{0}",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Implement INotifyPropertyChanged.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.ClassDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var type = (ITypeSymbol)context.ContainingSymbol;
            if (type.IsStatic ||
                type.Is(KnownSymbol.INotifyPropertyChanged) ||
                type.Is(KnownSymbol.MarkupExtension) ||
                type.Is(KnownSymbol.Attribute) ||
                type.Is(KnownSymbol.IValueConverter) ||
                type.Is(KnownSymbol.IMultiValueConverter) ||
                type.Is(KnownSymbol.DataTemplateSelector) ||
                type.Is(KnownSymbol.DependencyObject))
            {
                return;
            }

            var declaration = (ClassDeclarationSyntax)context.Node;
            if (declaration.Members.TryFirst(
                x => Property.ShouldNotify(x as PropertyDeclarationSyntax, context.SemanticModel, context.CancellationToken),
                out _))
            {
                var properties = string.Join(
                    Environment.NewLine,
                    declaration.Members.OfType<PropertyDeclarationSyntax>()
                               .Where(x => Property.ShouldNotify(x, context.SemanticModel, context.CancellationToken))
                               .Select(x => x.Identifier.ValueText));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.Identifier.GetLocation(), $"The class {type.Name} should notify for:{Environment.NewLine}{properties}"));
            }

            if (type.TryFindEvent("PropertyChanged", out var eventSymbol))
            {
                if (eventSymbol.Name != KnownSymbol.INotifyPropertyChanged.PropertyChanged.Name ||
                    eventSymbol.Type != KnownSymbol.PropertyChangedEventHandler ||
                    eventSymbol.IsStatic)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.Identifier.GetLocation(), $"The class {type.Name} has event PropertyChanged but does not implement INotifyPropertyChanged."));
            }
        }
    }
}
