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
                context.Node is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } } argument)
            {
                if (NameContext.Create(argument, context.SemanticModel, context.CancellationToken) is { Name: { } name, Expression: { } expression, Target: { ContainingSymbol: IMethodSymbol targetMethod } target })
                {
                    if (name == ContainingSymbolName(context.ContainingSymbol) &&
                        target.IsCallerMemberName())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, argument.GetLocation()));
                    }

                    if (!string.IsNullOrEmpty(name) &&
                        Notifies())
                    {
                        if (!Type().TryFindPropertyRecursive(name, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009NotifiesForMissingProperty, expression.GetLocation()));
                        }

                        if (argument.Expression is AnonymousFunctionExpressionSyntax)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC012DoNotUseExpression, argument.GetLocation()));
                        }

                        if (expression.IsKind(SyntaxKind.StringLiteralExpression) &&
                            SyntaxFacts.IsValidIdentifier(name) &&
                            context.ContainingSymbol.ContainingType.TryFindPropertyRecursive(name, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC013UseNameof, expression.GetLocation()));
                        }
                    }

                    bool Notifies()
                    {
                        return OnPropertyChanged.Match(targetMethod, context.SemanticModel, context.CancellationToken) is { } ||
                               TrySet.Match(targetMethod, context.SemanticModel, context.CancellationToken) is { } ||
                               targetMethod == KnownSymbol.PropertyChangedEventHandler.Invoke ||
                               EventHandlerOfPropertyChangedEventArgsInvoke();

                        bool EventHandlerOfPropertyChangedEventArgsInvoke()
                        {
                            return targetMethod.Name == "Invoke" &&
                                   targetMethod.ContainingType is { TypeArguments: { Length: 1 } typeArguments, MetadataName: "EventHandler`1" } &&
                                   typeArguments[0] == KnownSymbol.PropertyChangedEventArgs;
                        }
                    }

                    INamedTypeSymbol Type()
                    {
                        return invocation switch
                        {
                            { Parent: MemberAccessExpressionSyntax { Expression: InstanceExpressionSyntax _ } }
                            => context.ContainingSymbol.ContainingType,
                            { Expression: MemberAccessExpressionSyntax { Expression: { } e } }
                            when context.SemanticModel.TryGetNamedType(e, context.CancellationToken, out var type)
                            => type,
                            _ => context.ContainingSymbol.ContainingType,
                        };
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

        private readonly struct NameContext
        {
            internal readonly string? Name;
            internal readonly ExpressionSyntax Expression;
#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
            internal readonly IParameterSymbol Target;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

            private NameContext(string? name, ExpressionSyntax expression, IParameterSymbol target)
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
                    { Expression: AnonymousFunctionExpressionSyntax lambda }
                    when NameFromLambda(lambda) is { } name &&
                         Target() is { } target
                    => new NameContext(name.Identifier.ValueText, name, target),
                    { Expression: IdentifierNameSyntax cached }
                    when NameFromCached(cached) is { } name &&
                         Target() is { } target
                    => new NameContext(name, cached, target),
                    { Expression: MemberAccessExpressionSyntax cached }
                    when NameFromCached(cached) is { } name &&
                         Target() is { } target
                    => new NameContext(name, cached, target),
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

                static ExpressionSyntax Expression(ArgumentSyntax a)
                {
                    return a switch
                    {
                        { Expression: IdentifierNameSyntax name } => name,
                        { Expression: MemberAccessExpressionSyntax { Name: { } name } } => name,
                        _ => a.Expression,
                    };
                }

                static SimpleNameSyntax? NameFromLambda(AnonymousFunctionExpressionSyntax lambda)
                {
                    return lambda.Body switch
                    {
                        IdentifierNameSyntax name => name,
                        MemberAccessExpressionSyntax { Name: { } name } => name,
                        InvocationExpressionSyntax { Expression: IdentifierNameSyntax name } => name,
                        InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: { } name } } => name,
                        _ => null,
                    };
                }

                string? NameFromCached(ExpressionSyntax cached)
                {
                    return semanticModel.TryGetNamedType(cached, cancellationToken, out var type) &&
                           type == KnownSymbol.PropertyChangedEventArgs &&
                           PropertyChangedEventArgs.Match(cached, semanticModel, cancellationToken) is { } propertyChangedEventArgs &&
                           propertyChangedEventArgs.FindPropertyName(semanticModel, cancellationToken) is { Name: { } name }
                        ? name
                        : null;
                }
            }
        }
    }
}
