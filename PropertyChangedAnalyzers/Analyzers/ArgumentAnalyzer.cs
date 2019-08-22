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
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC004UseCallerMemberName,
            Descriptors.INPC009DoNotRaiseChangeForMissingProperty,
            Descriptors.INPC012DoNotUseExpression,
            Descriptors.INPC013UseNameof);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ArgumentSyntax argument &&
                argument.Parent is ArgumentListSyntax argumentList)
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
                            OnPropertyChanged.IsMatch(onPropertyChangedCandidate, context.SemanticModel, context.CancellationToken) != AnalysisResult.No &&
                            !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(text, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, argument.GetLocation()));
                        }

                        if (argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                            objectCreation.Parent is ArgumentSyntax parentArg &&
                            parentArg.FirstAncestor<InvocationExpressionSyntax>() is InvocationExpressionSyntax parentInvocation &&
                            context.SemanticModel.TryGetSymbol(objectCreation, KnownSymbol.PropertyChangedEventArgs, context.CancellationToken, out _))
                        {
                            if ((OnPropertyChanged.IsMatch(parentInvocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No ||
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

                if (argumentList.Parent is InvocationExpressionSyntax invocation &&
                    argument.Expression is AnonymousFunctionExpressionSyntax lambda &&
                    argumentList.Arguments.Count == 1 &&
                    TryGetNameFromLambda(lambda, out var lambdaName))
                {
                    if (OnPropertyChanged.IsMatch(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No)
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
                    if (invokeCandidate.ArgumentList?.Arguments.Count == 1 &&
                        OnPropertyChanged.IsMatch(invokeCandidate, context.SemanticModel, context.CancellationToken) != AnalysisResult.No &&
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
            if (symbol is IMethodSymbol method &&
                method.AssociatedSymbol is ISymbol associated)
            {
                return associated.Name;
            }

            return symbol.Name;
        }

        private static bool TryGetNameFromLambda(AnonymousFunctionExpressionSyntax lambda, out string name)
        {
            if (TryGetName(lambda.Body, out name))
            {
                return true;
            }

            if (lambda.Body is InvocationExpressionSyntax invocation)
            {
                return TryGetName(invocation.Expression, out name);
            }

            name = null;
            return false;

            bool TryGetName(SyntaxNode node, out string result)
            {
                result = null;
                if (node is IdentifierNameSyntax identifierName)
                {
                    result = identifierName.Identifier.ValueText;
                }

                if (node is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Name is SimpleNameSyntax nameSyntax)
                {
                    result = nameSyntax.Identifier.ValueText;
                }

                return result != null;
            }
        }
    }
}
