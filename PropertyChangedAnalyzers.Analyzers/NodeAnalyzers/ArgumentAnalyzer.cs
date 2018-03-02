namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ArgumentAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            INPC004UseCallerMemberName.Descriptor,
            INPC012DontUseExpression.Descriptor,
            INPC013UseNameof.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ArgumentSyntax argument)
            {
                if (argument.Parent?.Parent is InvocationExpressionSyntax invocation &&
                    invocation.ArgumentList is ArgumentListSyntax argumentList)
                {
                    if (argument.Expression.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
                    {
                        if (argumentList.Arguments.Count == 1)
                        {
                            var method = (IMethodSymbol)context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken);
                            if (PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC012DontUseExpression.Descriptor, context.Node.GetLocation()));
                            }
                        }
                    }

                    if (argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var text) &&
                        text == ContainingSymbolName(context.ContainingSymbol) &&
                        context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol invokedMethod &&
                        invokedMethod.TryGetMatchingParameter(argument, out var parameter) &&
                        parameter.IsCallerMemberName())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC004UseCallerMemberName.Descriptor, argument.GetLocation()));
                    }
                }

                if (argument.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                    SyntaxFacts.IsValidIdentifier(literal.Token.ValueText))
                {
                    if (context.ContainingSymbol is IMethodSymbol method &&
                        method.Parameters.TrySingle(x => x.Name == literal.Token.ValueText, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC013UseNameof.Descriptor, argument.GetLocation()));
                    }

                    if (context.ContainingSymbol.ContainingType.TryGetProperty(literal.Token.ValueText, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC013UseNameof.Descriptor, argument.GetLocation()));
                    }
                }
            }
        }

        private static string ContainingSymbolName(ISymbol symbol)
        {
            if (symbol is IMethodSymbol method &&
                method.AssociatedSymbol is ISymbol associated)
            {
                return associated.Name;
            }

            return symbol.Name;
        }
    }
}
