﻿namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class StructAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.INPC008StructMustNotNotify);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.StructDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is StructDeclarationSyntax { BaseList: { } baseList } declaration &&
                context.ContainingSymbol is INamedTypeSymbol type &&
                type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC008StructMustNotNotify, Location(), context.ContainingSymbol.Name));

                Location Location()
                {
                    foreach (var baseType in baseList.Types)
                    {
                        if (baseType == KnownSymbol.INotifyPropertyChanged)
                        {
                            return baseType.GetLocation();
                        }
                    }

                    return declaration.GetLocation();
                }
            }
        }
    }
}
