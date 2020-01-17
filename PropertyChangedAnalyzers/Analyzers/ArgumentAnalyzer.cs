namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
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
            Descriptors.INPC009NotifiesForMissingProperty,
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
                if (NameContext.Create(argument, context.SemanticModel, context.CancellationToken) is { Name: { } name, Expression: { } expression, Target: { ContainingSymbol: IMethodSymbol targetMethod } target })
                {
                    if (name == ContainingSymbolName(context.ContainingSymbol) &&
                        target.IsCallerMemberName())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, argument.GetLocation()));
                    }

                    if (!string.IsNullOrEmpty(name) &&
                        (OnPropertyChanged.Match(targetMethod, context.SemanticModel, context.CancellationToken) is { } ||
                         targetMethod == KnownSymbol.PropertyChangedEventHandler.Invoke) &&
                        !Type().TryFindPropertyRecursive(name, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009NotifiesForMissingProperty, expression.GetLocation()));
                    }

                    INamedTypeSymbol Type()
                    {
                        return argumentList switch
                        {
                            { Parent: InvocationExpressionSyntax { Parent: MemberAccessExpressionSyntax { Expression: InstanceExpressionSyntax _ } } }
                            => context.ContainingSymbol.ContainingType,
                            { Parent: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } e } } }
                            when context.SemanticModel.TryGetNamedType(e, context.CancellationToken, out var type)
                            => type,
                            _ => context.ContainingSymbol.ContainingType,
                        };
                    }
                }

                if (argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var text))
                {
                    if (SyntaxFacts.IsValidIdentifier(text))
                    {
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
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009NotifiesForMissingProperty, argument.GetLocation()));
                        }
                    }
                }

                if ((argument.Expression is IdentifierNameSyntax ||
                    argument.Expression is MemberAccessExpressionSyntax) &&
                    argumentList.Parent is InvocationExpressionSyntax invokeCandidate)
                {
                    if (argumentList.Arguments.Count == 1 &&
                        OnPropertyChanged.Match(invokeCandidate, context.SemanticModel, context.CancellationToken) is { } &&
                        IsMissing())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009NotifiesForMissingProperty, argument.GetLocation()));
                    }

                    if (PropertyChangedEvent.IsInvoke(invokeCandidate, context.SemanticModel, context.CancellationToken) &&
                        argumentList.Arguments[1] == argument &&
                        context.SemanticModel.TryGetSymbol(invokeCandidate, context.CancellationToken, out _) &&
                        IsMissing())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009NotifiesForMissingProperty, argument.GetLocation()));
                    }

                    bool IsMissing()
                    {
                        return PropertyChanged.FindPropertyName(invokeCandidate, context.SemanticModel, context.CancellationToken) is { Value: var propertyName } &&
                               !string.IsNullOrEmpty(propertyName) &&
                               !context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(propertyName, out _);
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

        private struct NameContext
        {
            internal readonly string Name;
            internal readonly ExpressionSyntax Expression;
#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
            internal readonly IParameterSymbol Target;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

            internal NameContext(string name, ExpressionSyntax expression, IParameterSymbol target)
            {
                this.Name = name;
                this.Expression = expression;
                this.Target = target;
            }

            internal static NameContext? Create(ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                return argument switch
                {
                    { Expression: LiteralExpressionSyntax literal }
                    when literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                         argument.TryGetStringValue(semanticModel, cancellationToken, out var text) &&
                         Target() is { } target
                    => new NameContext(text, literal, target),
                    { Expression: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier: { ValueText: "nameof" } }, ArgumentList: { Arguments: { Count: 1 } arguments } } }
                    when argument.TryGetStringValue(semanticModel, cancellationToken, out var text) &&
                         Target() is { } target
                    => new NameContext(text, Expression(arguments[0]), target),
                    { Expression: ObjectCreationExpressionSyntax { ArgumentList: { Arguments: { Count: 1 } arguments } } objectCreation }
                    when objectCreation.Type.IsSameType(KnownSymbol.PropertyChangedEventArgs, semanticModel) &&
                         Create(arguments[0], semanticModel, cancellationToken) is { Name: { } name, Expression: { } expression } &&
                         Target() is { } target
                    => new NameContext(name, expression, target),
                    _ => null,
                };

                IParameterSymbol? Target()
                {
                    if (argument is { Parent: ArgumentListSyntax { Parent: ExpressionSyntax parent } } &&
                        semanticModel.TryGetSymbol(parent, cancellationToken, out IMethodSymbol? method) &&
                        method.TryFindParameter(argument, out var parameter))
                    {
                        return parameter;
                    }

                    return null;
                }

                ExpressionSyntax Expression(ArgumentSyntax a)
                {
                    return a switch
                    {
                        { Expression: IdentifierNameSyntax identifierName } => identifierName,
                        { Expression: MemberAccessExpressionSyntax { Name: { } name } } => name,
                        _ => a.Expression,
                    };
                }
            }
        }
    }
}
