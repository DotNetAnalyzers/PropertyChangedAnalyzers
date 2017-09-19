namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC004UseCallerMemberName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC004";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use [CallerMemberName]",
            messageFormat: "Use [CallerMemberName]",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Use [CallerMemberName]",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var method = (IMethodSymbol)context.ContainingSymbol;
            if (method.Parameters.Length != 1 ||
                method.Parameters[0].Type != KnownSymbol.String ||
                method.Parameters[0].IsCallerMemberName())
            {
                return;
            }

            if (PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) == PropertyChangedAnalyzers.AnalysisResult.Yes)
            {
                var methodDeclaration = (MethodDeclarationSyntax)context.Node;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.ParameterList.Parameters[0].GetLocation()));
            }
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var property = (context.ContainingSymbol as IMethodSymbol)?.AssociatedSymbol as IPropertySymbol;
            if (property == null)
            {
                return;
            }

            var invocation = (InvocationExpressionSyntax)context.Node;
            if (invocation.ArgumentList?.Arguments.Count != 1)
            {
                return;
            }

            var method = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;
            if (PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) != PropertyChangedAnalyzers.AnalysisResult.Yes)
            {
                return;
            }

            var argument = invocation.ArgumentList.Arguments[0];
            if (argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out string text) &&
                text == property.Name)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation()));
            }
        }
    }
}