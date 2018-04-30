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

            if (context.ContainingSymbol is INamedTypeSymbol type &&
                context.Node is ClassDeclarationSyntax classDeclaration)
            {
                if (type.IsStatic ||
                    type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation) ||
                    type.IsAssignableTo(KnownSymbol.MarkupExtension, context.Compilation) ||
                    type.IsAssignableTo(KnownSymbol.Attribute, context.Compilation) ||
                    type.IsAssignableTo(KnownSymbol.IValueConverter, context.Compilation) ||
                    type.IsAssignableTo(KnownSymbol.IMultiValueConverter, context.Compilation) ||
                    type.IsAssignableTo(KnownSymbol.DataTemplateSelector, context.Compilation) ||
                    type.IsAssignableTo(KnownSymbol.DependencyObject, context.Compilation))
                {
                    return;
                }

                if (classDeclaration.Members.TryFirstOfType(x => Property.ShouldNotify(x, context.SemanticModel, context.CancellationToken), out PropertyDeclarationSyntax _))
                {
                    var properties = string.Join(
                        Environment.NewLine,
                        classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                                   .Where(x => Property.ShouldNotify(x, context.SemanticModel, context.CancellationToken))
                                   .Select(x => x.Identifier.ValueText));
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation(), $"The class {type.Name} should notify for:{Environment.NewLine}{properties}"));
                }

                if (type.TryFindEvent("PropertyChanged", out var eventSymbol))
                {
                    if (eventSymbol.Name != KnownSymbol.INotifyPropertyChanged.PropertyChanged.Name ||
                        eventSymbol.Type != KnownSymbol.PropertyChangedEventHandler ||
                        eventSymbol.IsStatic)
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation(), $"The class {type.Name} has event PropertyChanged but does not implement INotifyPropertyChanged."));
                }
            }
        }
    }
}
