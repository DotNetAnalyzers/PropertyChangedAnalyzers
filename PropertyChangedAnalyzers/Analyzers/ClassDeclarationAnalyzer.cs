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
    internal class ClassDeclarationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC001ImplementINotifyPropertyChanged);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.ClassDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is INamedTypeSymbol { IsStatic: false } type &&
                context.Node is ClassDeclarationSyntax classDeclaration &&
                !IsExcludedType())
            {
                if (classDeclaration.Members.TryFirstOfType(x => Property.ShouldNotify(x, context.SemanticModel, context.CancellationToken), out PropertyDeclarationSyntax _))
                {
                    var properties = string.Join(
                        Environment.NewLine,
                        classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                                   .Where(x => Property.ShouldNotify(x, context.SemanticModel, context.CancellationToken))
                                   .Select(x => x.Identifier.ValueText));
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.INPC001ImplementINotifyPropertyChanged,
                            classDeclaration.Identifier.GetLocation(),
                            $"The class {type.Name} should notify for:{Environment.NewLine}{properties}"));
                }

                if (PropertyChangedEvent.Find(type) is { IsStatic: false })
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.INPC001ImplementINotifyPropertyChanged,
                            classDeclaration.Identifier.GetLocation(),
                            $"The class {type.Name} has event PropertyChanged but does not implement INotifyPropertyChanged."));
                }
            }

            bool IsExcludedType()
            {
                return type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.Attribute, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.IEnumerator, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.Stream, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.MarkupExtension, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.IValueConverter, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.IMultiValueConverter, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.DataTemplateSelector, context.Compilation) ||
                       type.IsAssignableTo(KnownSymbol.DependencyObject, context.Compilation);
            }
        }
    }
}
