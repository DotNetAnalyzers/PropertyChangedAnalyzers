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
            INPC009DontRaiseChangeForMissingProperty.Descriptor,
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

            if (context.Node is ArgumentSyntax argument &&
                argument.Parent is ArgumentListSyntax argumentList)
            {
                if (argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var text) &&
                    context.SemanticModel.GetSymbolSafe(argumentList.Parent, context.CancellationToken) is IMethodSymbol method &&
                    method.TryGetMatchingParameter(argument, out var parameter))
                {
                    if (text == ContainingSymbolName(context.ContainingSymbol) &&
                        parameter.IsCallerMemberName())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC004UseCallerMemberName.Descriptor, argument.GetLocation()));
                    }

                    if (SyntaxFacts.IsValidIdentifier(text))
                    {
                        if (argumentList.Parent is InvocationExpressionSyntax &&
                            !method.ContainingType.TryGetProperty(text, out _) &&
                            PropertyChanged.IsPropertyChangedInvoker(method, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                        }

                        if (argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                            objectCreation.Parent is ArgumentSyntax parentArg &&
                            method.MethodKind == MethodKind.Constructor &&
                            method.ContainingType == KnownSymbol.PropertyChangedEventArgs &&
                            context.SemanticModel.GetSymbolSafe(parentArg.FirstAncestor<InvocationExpressionSyntax>(), context.CancellationToken) is IMethodSymbol parentMethod)
                        {
                            if (!parentMethod.ContainingType.TryGetProperty(text, out _) &&
                                PropertyChanged.IsPropertyChangedInvoker(parentMethod, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                            }

                            if (parentMethod == KnownSymbol.PropertyChangedEventHandler.Invoke &&
                                !context.ContainingSymbol.ContainingType.TryGetProperty(text, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                            }
                        }
                    }
                }

                if (argumentList.Parent is InvocationExpressionSyntax invocation &&
                    argument.Expression is AnonymousFunctionExpressionSyntax lambda &&
                    argumentList.Arguments.Count == 1 &&
                    context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol candidate)
                {
                    if (PropertyChanged.IsPropertyChangedInvoker(candidate, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC012DontUseExpression.Descriptor, argument.GetLocation()));
                        if (TryGetNameFromLambda(lambda, out var lambdaName))
                        {
                            if (!candidate.ContainingType.TryGetProperty(lambdaName, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                            }
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                        }
                    }
                }

                if (argument.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                    SyntaxFacts.IsValidIdentifier(literal.Token.ValueText))
                {
                    if (context.ContainingSymbol is IMethodSymbol containingMethod &&
                        containingMethod.Parameters.TrySingle(x => x.Name == literal.Token.ValueText, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC013UseNameof.Descriptor, argument.GetLocation()));
                    }

                    if (context.ContainingSymbol.ContainingType.TryGetProperty(literal.Token.ValueText, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC013UseNameof.Descriptor, argument.GetLocation()));
                    }
                }

                if ((argument.Expression is IdentifierNameSyntax ||
                    argument.Expression is MemberAccessExpressionSyntax) &&
                    argumentList.Parent is InvocationExpressionSyntax invokeCandidate)
                {
                    if (invokeCandidate.ArgumentList?.Arguments.Count == 1 &&
                        context.SemanticModel.GetSymbolSafe(invokeCandidate, context.CancellationToken) is IMethodSymbol invoker &&
                        PropertyChanged.IsPropertyChangedInvoker(invoker, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes &&
                        PropertyChanged.TryGetInvokedPropertyChangedName(invokeCandidate, context.SemanticModel, context.CancellationToken, out _, out var propertyName) == AnalysisResult.Yes &&
                        !string.IsNullOrEmpty(propertyName) &&
                        !context.ContainingSymbol.ContainingType.TryGetProperty(propertyName, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                    }

                    if (invokeCandidate.TryGetInvokedMethodName(out var name) &&
                         name == "Invoke" &&
                         invokeCandidate.ArgumentList?.Arguments.Count == 2 &&
                         argumentList.Arguments[1] == argument &&
                         context.SemanticModel.GetSymbolSafe(invokeCandidate, context.CancellationToken) is IMethodSymbol &&
                         PropertyChanged.TryGetInvokedPropertyChangedName(invokeCandidate, context.SemanticModel, context.CancellationToken, out _, out propertyName) == AnalysisResult.Yes &&
                         !string.IsNullOrEmpty(propertyName) &&
                         !context.ContainingSymbol.ContainingType.TryGetProperty(propertyName, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
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
            name = null;
            if (lambda.Body is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
            }

            if (lambda.Body is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is SimpleNameSyntax nameSyntax)
            {
                name = nameSyntax.Identifier.ValueText;
            }

            return name != null;
        }
    }
}
