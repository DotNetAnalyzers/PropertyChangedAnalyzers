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
                if (argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var text))
                {
                    if (text == ContainingSymbolName(context.ContainingSymbol) &&
                        context.SemanticModel.GetSymbolSafe(argumentList.Parent, context.CancellationToken) is IMethodSymbol method &&
                        method.TryGetMatchingParameter(argument, out var parameter))
                    {
                        if (parameter.IsCallerMemberName())
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC004UseCallerMemberName.Descriptor, argument.GetLocation()));
                        }
                        else if (parameter.TrySingleDeclaration<SyntaxNode>(context.CancellationToken, out _) &&
                                 PropertyChanged.IsOnPropertyChanged(method, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC004UseCallerMemberName.Descriptor, argument.GetLocation()));
                        }
                    }

                    if (SyntaxFacts.IsValidIdentifier(text))
                    {
                        if (argumentList.Parent is InvocationExpressionSyntax onPropertyChangedCandidate &&
                            PropertyChanged.IsOnPropertyChanged(onPropertyChangedCandidate, context.SemanticModel, context.CancellationToken) != AnalysisResult.No &&
                            !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(text, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                        }

                        if (argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                            objectCreation.Parent is ArgumentSyntax parentArg &&
                            parentArg.FirstAncestor<InvocationExpressionSyntax>() is InvocationExpressionSyntax parentInvocation &&
                            Constructor.TryGet(objectCreation, KnownSymbol.PropertyChangedEventArgs, context.SemanticModel, context.CancellationToken, out _))
                        {
                            if ((PropertyChanged.IsOnPropertyChanged(parentInvocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No ||
                                 PropertyChanged.IsPropertyChangedInvoke(parentInvocation, context.SemanticModel, context.CancellationToken)) &&
                                !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(text, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                            }
                        }

                        if (argument.Expression is LiteralExpressionSyntax literal &&
                            literal.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            if (context.ContainingSymbol is IMethodSymbol containingMethod &&
                                containingMethod.Parameters.TrySingle(x => x.Name == literal.Token.ValueText, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC013UseNameof.Descriptor, argument.GetLocation()));
                            }

                            if (context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(literal.Token.ValueText, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC013UseNameof.Descriptor, argument.GetLocation()));
                            }
                        }
                    }
                }

                if (argumentList.Parent is InvocationExpressionSyntax invocation &&
                    argument.Expression is AnonymousFunctionExpressionSyntax lambda &&
                    argumentList.Arguments.Count == 1 &&
                    TryGetNameFromLambda(lambda, out var lambdaName))
                {
                    if (PropertyChanged.IsOnPropertyChanged(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC012DontUseExpression.Descriptor, argument.GetLocation()));
                        if (!context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(lambdaName, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                        }
                    }
                }

                if ((argument.Expression is IdentifierNameSyntax ||
                    argument.Expression is MemberAccessExpressionSyntax) &&
                    argumentList.Parent is InvocationExpressionSyntax invokeCandidate)
                {
                    if (invokeCandidate.ArgumentList?.Arguments.Count == 1 &&
                        PropertyChanged.IsOnPropertyChanged(invokeCandidate, context.SemanticModel, context.CancellationToken) != AnalysisResult.No &&
                        PropertyChanged.TryGetInvokedPropertyChangedName(invokeCandidate, context.SemanticModel, context.CancellationToken, out var propertyName) == AnalysisResult.Yes &&
                        !string.IsNullOrEmpty(propertyName) &&
                        !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(propertyName, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, argument.GetLocation()));
                    }

                    if (PropertyChanged.IsPropertyChangedInvoke(invokeCandidate, context.SemanticModel, context.CancellationToken) &&
                         argumentList.Arguments[1] == argument &&
                         context.SemanticModel.GetSymbolSafe(invokeCandidate, context.CancellationToken) is IMethodSymbol &&
                         PropertyChanged.TryGetInvokedPropertyChangedName(invokeCandidate, context.SemanticModel, context.CancellationToken, out propertyName) == AnalysisResult.Yes &&
                         !string.IsNullOrEmpty(propertyName) &&
                         !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(propertyName, out _))
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
