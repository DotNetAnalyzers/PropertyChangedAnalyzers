namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ArgumentAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC004UseCallerMemberName,
            Descriptors.INPC009DoNotRaiseChangeForMissingProperty,
            Descriptors.INPC012DoNotUseExpression,
            Descriptors.INPC013UseNameof);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ArgumentSyntax { Parent: ArgumentListSyntax argumentList } argument)
            {
                if (argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var text))
                {
                    if (text == ContainingSymbolName(context.ContainingSymbol) &&
                        context.SemanticModel.GetSymbolSafe(argumentList.Parent, context.CancellationToken) is IMethodSymbol method &&
                        method.TryFindParameter(argument, out var parameter))
                    {
                        if (parameter.IsCallerMemberName())
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, argument.GetLocation()));
                        }
                        else if (parameter.TrySingleDeclaration<SyntaxNode>(context.CancellationToken, out _) &&
                                 OnPropertyChanged.IsMatch(method, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, argument.GetLocation()));
                        }
                    }

                    if (SyntaxFacts.IsValidIdentifier(text))
                    {
                        if (argumentList.Parent is InvocationExpressionSyntax onPropertyChangedCandidate &&
                            OnPropertyChanged.Match(onPropertyChangedCandidate, context.SemanticModel, context.CancellationToken) is { } &&
                            !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(text, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, argument.GetLocation()));
                        }

                        if (argumentList.Parent is ObjectCreationExpressionSyntax { Parent: ArgumentSyntax parentArg } objectCreation &&
                            parentArg.FirstAncestor<InvocationExpressionSyntax>() is { } parentInvocation &&
                            context.SemanticModel.TryGetSymbol(objectCreation, KnownSymbol.PropertyChangedEventArgs, context.CancellationToken, out _))
                        {
                            if ((OnPropertyChanged.Match(parentInvocation, context.SemanticModel, context.CancellationToken) is { } ||
                                 PropertyChangedEvent.IsInvoke(parentInvocation, context.SemanticModel, context.CancellationToken)) &&
                                !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(text, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, argument.GetLocation()));
                            }
                        }

                        if (argument.Expression is LiteralExpressionSyntax literal &&
                            literal.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            if (context.ContainingSymbol is IMethodSymbol containingMethod &&
                                containingMethod.Parameters.TrySingle(x => x.Name == literal.Token.ValueText, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC013UseNameof, argument.GetLocation()));
                            }

                            if (context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(literal.Token.ValueText, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC013UseNameof, argument.GetLocation()));
                            }
                        }
                    }
                }

                if (argument is { Expression: AnonymousFunctionExpressionSyntax lambda } &&
                    argumentList is { Arguments: { Count: 1 }, Parent: InvocationExpressionSyntax invocation } &&
                    GetNameFromLambda(lambda) is { } lambdaName)
                {
                    if (OnPropertyChanged.Match(invocation, context.SemanticModel, context.CancellationToken) is { })
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC012DoNotUseExpression, argument.GetLocation()));
                        if (!context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(lambdaName, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, argument.GetLocation()));
                        }
                    }
                }

                if ((argument.Expression is IdentifierNameSyntax ||
                    argument.Expression is MemberAccessExpressionSyntax) &&
                    argumentList.Parent is InvocationExpressionSyntax invokeCandidate)
                {
                    if (argumentList.Arguments.Count == 1 &&
                        OnPropertyChanged.Match(invokeCandidate, context.SemanticModel, context.CancellationToken) is { } &&
                        PropertyChanged.TryGetName(invokeCandidate, context.SemanticModel, context.CancellationToken, out var propertyName) == AnalysisResult.Yes &&
                        !string.IsNullOrEmpty(propertyName) &&
                        !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(propertyName, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, argument.GetLocation()));
                    }

                    if (PropertyChangedEvent.IsInvoke(invokeCandidate, context.SemanticModel, context.CancellationToken) &&
                         argumentList.Arguments[1] == argument &&
                         context.SemanticModel.TryGetSymbol(invokeCandidate, context.CancellationToken, out _) &&
                         PropertyChanged.TryGetName(invokeCandidate, context.SemanticModel, context.CancellationToken, out propertyName) == AnalysisResult.Yes &&
                         !string.IsNullOrEmpty(propertyName) &&
                         !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(propertyName, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, argument.GetLocation()));
                    }
                }
            }
        }

        private static string ContainingSymbolName(ISymbol symbol)
        {
            if (symbol is IMethodSymbol { AssociatedSymbol: { } associated })
            {
                return associated.Name;
            }

            return symbol.Name;
        }

        private static string? GetNameFromLambda(AnonymousFunctionExpressionSyntax lambda)
        {
            return TryGetName(lambda.Body);

            static string? TryGetName(SyntaxNode node)
            {
                return node switch
                {
                    IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                    MemberAccessExpressionSyntax { Name: { } memberName } => memberName.Identifier.ValueText,
                    InvocationExpressionSyntax { Expression: { } expression } => TryGetName(expression),
                    _ => null,
                };
            }
        }
    }
}
