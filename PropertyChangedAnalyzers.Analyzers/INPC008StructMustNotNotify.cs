﻿namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC008StructMustNotNotify : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC008";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Struct must not implement INotifyPropertyChanged",
            messageFormat: "Struct '{0}' implements INotifyPropertyChanged",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Struct must not implement INotifyPropertyChanged",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.StructDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var structDeclaration = (StructDeclarationSyntax)context.Node;
            if (structDeclaration.ImplementsINotifyPropertyChanged())
            {
                var type = structDeclaration?.BaseList?.Types.First(x => x.IsINotifyPropertyChanged());
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, type?.GetLocation() ?? structDeclaration?.GetLocation(), context.ContainingSymbol.Name));
            }
        }
    }
}