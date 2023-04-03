namespace PropertyChangedAnalyzers;

using System.Collections.Immutable;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MethodDeclarationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.INPC004UseCallerMemberName,
        Descriptors.INPC018InvokerShouldBeProtected);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.MethodDeclaration);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.ContainingSymbol is IMethodSymbol method &&
            context.Node is MethodDeclarationSyntax methodDeclaration)
        {
            if (OnPropertyChanged.Match(method, context.SemanticModel, context.CancellationToken) is { AnalysisResult: AnalysisResult.Yes, Name: { } parameter })
            {
                if (ShouldBeCallerMemberName(parameter) is { } parameterLocation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, parameterLocation));
                }

                if (ShouldBeProtected() is { } location)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.INPC018InvokerShouldBeProtected,
                            location));
                }
            }
            else if (TrySet.Match(method, context.SemanticModel, context.CancellationToken) is { AnalysisResult: AnalysisResult.Yes, Name: { } nameParameter })
            {
                if (ShouldBeCallerMemberName(nameParameter) is { } parameterLocation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, parameterLocation));
                }

                if (ShouldBeProtected() is { } location)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.INPC018InvokerShouldBeProtected,
                            location));
                }
            }
        }

        Location? ShouldBeCallerMemberName(IParameterSymbol candidate)
        {
            return !candidate.IsCallerMemberName() &&
                   candidate.Type is { SpecialType: SpecialType.System_String } &&
                   CallerMemberNameAttribute.IsAvailable(context.SemanticModel) &&
                   candidate.TrySingleDeclaration(context.CancellationToken, out var declaration)
                ? declaration.GetLocation()
                : null;
        }

        Location? ShouldBeProtected()
        {
            if (method is { DeclaredAccessibility: Accessibility.Private, ContainingType: { IsSealed: false, IsStatic: false } })
            {
                return methodDeclaration.Modifiers.TryFirst(x => x.IsKind(SyntaxKind.PrivateKeyword), out var modifier)
                    ? modifier.GetLocation()
                    : methodDeclaration.Identifier.GetLocation();
            }

            return null;
        }
    }
}
