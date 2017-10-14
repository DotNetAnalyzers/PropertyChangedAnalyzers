namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC009DontRaiseChangeForMissingProperty : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC009";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't raise PropertyChanged for missing property.",
            messageFormat: "Don't raise PropertyChanged for missing property.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Don't raise PropertyChanged for missing property.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is IFieldSymbol)
            {
                return;
            }

            var invocation = (InvocationExpressionSyntax)context.Node;
            if (invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count > 2)
            {
                return;
            }

            var method = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;
            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke ||
                PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) != AnalysisResult.No)
            {
                if (PropertyChanged.TryGetInvokedPropertyChangedName(invocation, context.SemanticModel, context.CancellationToken, out var nameArg, out var propertyName) == AnalysisResult.Yes)
                {
                    var type = invocation.Expression is IdentifierNameSyntax ||
                               (invocation.Expression as MemberAccessExpressionSyntax)?.Expression is ThisExpressionSyntax ||
                               (invocation.Expression as MemberAccessExpressionSyntax)?.Expression is BaseExpressionSyntax ||
                               context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol == KnownSymbol.PropertyChangedEventHandler.Invoke
                        ? context.ContainingSymbol.ContainingType
                        : context.SemanticModel.GetTypeInfoSafe((invocation.Expression as MemberAccessExpressionSyntax)?.Expression, context.CancellationToken)
                                 .Type;
                    if (type.TryGetProperty(propertyName, out _))
                    {
                        return;
                    }

                    if (invocation.Span.Contains(nameArg.Span))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameArg.GetLocation()));
                        return;
                    }

                    if (method == KnownSymbol.PropertyChangedEventHandler.Invoke)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.ArgumentList.Arguments[1].GetLocation()));
                        return;
                    }

                    if (invocation.ArgumentList.Arguments.Count == 1)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.ArgumentList.Arguments[0].GetLocation()));
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation()));
                }
            }
        }
    }
}