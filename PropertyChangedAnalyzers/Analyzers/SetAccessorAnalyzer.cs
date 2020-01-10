namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class SetAccessorAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC002MutablePublicPropertyShouldNotify,
            Descriptors.INPC016NotifyAfterMutation);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.SetAccessorDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AccessorDeclarationSyntax { Parent: AccessorListSyntax { Parent: PropertyDeclarationSyntax containingProperty } } setter &&
                !IsBindableFalse() &&
                context.ContainingSymbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } &&
                property.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation))
            {
                switch (setter)
                {
                    case { ExpressionBody: { Expression: { } expression } }:
                        if (expression.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                            Property.ShouldNotify(containingProperty, property, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                    containingProperty.Identifier.GetLocation(),
                                    property.Name));
                        }

                        break;
                    case { Body: { } body }:
                        if (Property.ShouldNotify(containingProperty, property, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                    containingProperty.Identifier.GetLocation(),
                                    property.Name));
                        }

                        Walk(body);
                        break;
                    case { Body: null, ExpressionBody: null }:
                        if (Property.ShouldNotify(containingProperty, property, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                    setter.SyntaxTree.GetLocation(TextSpan.FromBounds(containingProperty.Identifier.SpanStart, containingProperty.Span.End)),
                                    property.Name));
                        }

                        break;
                }

                void Walk(BlockSyntax body)
                {
                    ExpressionSyntax? backing = null;
                    foreach (var statement in body.Statements)
                    {
                        switch (statement)
                        {
                            case IfStatementSyntax { Condition: { } condition }
                                when Gu.Roslyn.AnalyzerExtensions.Equality.IsEqualsCheck(condition, context.SemanticModel, context.CancellationToken, out _, out _):
                                break;
                            case ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax { Left: { } left, Right: IdentifierNameSyntax { Identifier: { ValueText: "value" } } } }:
                                backing = left;
                                break;
                            case ExpressionStatementSyntax { Expression: InvocationExpressionSyntax { ArgumentList: { Arguments: { } arguments } } invocation }:
                                if (TrySet.IsMatch(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No &&
                                    arguments.TrySingle<ArgumentSyntax>(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword), out var temp))
                                {
                                    backing = temp.Expression;
                                }
                                else if (backing == null &&
                                    (OnPropertyChanged.IsMatch(invocation, context.SemanticModel, context.CancellationToken, out _) != AnalysisResult.No ||
                                     PropertyChangedEvent.IsInvoke(invocation, context.SemanticModel, context.CancellationToken)))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC016NotifyAfterMutation, statement.GetLocation()));
                                }

                                break;
                            case ExpressionStatementSyntax { Expression: ConditionalAccessExpressionSyntax { WhenNotNull: InvocationExpressionSyntax invocation } }:
                                if (backing == null &&
                                    PropertyChangedEvent.IsInvoke(invocation, context.SemanticModel, context.CancellationToken))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC016NotifyAfterMutation, statement.GetLocation()));
                                }

                                break;
                            default:
                                return;
                        }
                    }
                }
            }

            bool IsBindableFalse()
            {
                if (Attribute.TryFind(containingProperty, KnownSymbol.BindableAttribute, context.SemanticModel, context.CancellationToken, out var bindable))
                {
                    return bindable is { ArgumentList: { Arguments: { Count: 1 } arguments } } &&
                           arguments[0] is { Expression: LiteralExpressionSyntax { Token: { ValueText: "false" } } };
                }

                return false;
            }
        }
    }
}
