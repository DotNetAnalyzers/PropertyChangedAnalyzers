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
            Descriptors.INPC005CheckIfDifferentBeforeNotifying,
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
                context.ContainingSymbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property, ContainingType: { } containingType } &&
                ShouldCheck())
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

                        HandleBody(body, context);
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
            }

            bool ShouldCheck()
            {
                if (property.IsStatic &&
                    PropertyChangedEvent.Find(containingType) is null)
                {
                    return false;
                }

                if (!property.IsStatic &&
                    !containingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation))
                {
                    return false;
                }

                if (Attribute.TryFind(containingProperty, KnownSymbol.BindableAttribute, context.SemanticModel, context.CancellationToken, out var bindable) &&
                    bindable is { ArgumentList: { Arguments: { Count: 1 } arguments } } &&
                    arguments[0] is { Expression: LiteralExpressionSyntax { Token: { ValueText: "false" } } })
                {
                    return false;
                }

                return true;
            }
        }

        private static void HandleBody(BlockSyntax body, SyntaxNodeAnalysisContext context)
        {
            ExpressionSyntax? backing = null;
            bool? equals = null;

            _ = Walk(body);

            bool Walk(StatementSyntax statement)
            {
                switch (statement)
                {
                    case BlockSyntax block:
                        foreach (var blockStatement in block.Statements)
                        {
                            if (!Handle(blockStatement))
                            {
                                return false;
                            }
                        }

                        return true;
                    default:
                        return Handle(statement);
                }
            }

            bool Handle(StatementSyntax statement)
            {
                switch (statement)
                {
                    case IfStatementSyntax { Condition: { } condition } ifStatement
                        when Equality.IsEqualsCheck(condition, context.SemanticModel, context.CancellationToken, out _, out _):
                        equals = Equals(condition);
                        return WalkIfStatement(ifStatement);

                    case IfStatementSyntax { Condition: PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: { } condition } } ifStatement
                        when Equality.IsEqualsCheck(condition, context.SemanticModel, context.CancellationToken, out _, out _):
                        equals = !Equals(condition);
                        return WalkIfStatement(ifStatement);

                    case IfStatementSyntax { Condition: InvocationExpressionSyntax trySet } ifStatement
                        when TrySet.Match(trySet, context.SemanticModel, context.CancellationToken) is { Field: { Expression: { } field } }:
                        backing = field;
                        equals = false;
                        return WalkIfStatement(ifStatement);
                    case IfStatementSyntax { Condition: PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: InvocationExpressionSyntax trySet } } ifStatement
                        when TrySet.Match(trySet, context.SemanticModel, context.CancellationToken) is { Field: { Expression: { } field } }:
                        backing = field;
                        equals = true;
                        return WalkIfStatement(ifStatement);
                    case ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax { Left: { } left } }:
                        backing = left;
                        return true;
                    case ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation }
                        when TrySet.Match(invocation, context.SemanticModel, context.CancellationToken) is { Field: { Expression: { } field } }:
                        backing = field;
                        return true;
                    case ExpressionStatementSyntax { Expression: ConditionalAccessExpressionSyntax { WhenNotNull: InvocationExpressionSyntax conditionalInvoke } }
                        when PropertyChangedEvent.IsInvoke(conditionalInvoke, context.SemanticModel, context.CancellationToken):
                    case ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invoke }
                        when PropertyChangedEvent.IsInvoke(invoke, context.SemanticModel, context.CancellationToken):
                    case ExpressionStatementSyntax { Expression: InvocationExpressionSyntax onPropertyChanged }
                        when OnPropertyChanged.Match(onPropertyChanged, context.SemanticModel, context.CancellationToken) is { }:
                        if (backing is null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC016NotifyAfterMutation, statement.GetLocation()));
                        }

                        if (equals != false &&
                            !IsPreviousStatementNotify())
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC005CheckIfDifferentBeforeNotifying, statement.GetLocation()));
                        }

                        return true;

                        bool IsPreviousStatementNotify()
                        {
                            if (statement.Parent is BlockSyntax { Statements: { } statements } &&
                                statements.TryElementAt(statements.IndexOf(statement) - 1, out var previous))
                            {
                                return previous switch
                                {
                                    ExpressionStatementSyntax { Expression: ConditionalAccessExpressionSyntax { WhenNotNull: InvocationExpressionSyntax invocation } }
                                    => PropertyChangedEvent.IsInvoke(invocation, context.SemanticModel, context.CancellationToken),
                                    ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation }
                                    => PropertyChangedEvent.IsInvoke(invocation, context.SemanticModel, context.CancellationToken) ||
                                       OnPropertyChanged.Match(invocation, context.SemanticModel, context.CancellationToken) is { },
                                    _ => false,
                                };
                            }

                            return false;
                        }

                    case BlockSyntax block:
                        return Walk(block);
                    case LockStatementSyntax { Statement: { } lockStatement }:
                        return Walk(lockStatement);
                    case ThrowStatementSyntax _:
                    case ReturnStatementSyntax _:
                    case EmptyStatementSyntax _:
                        return true;
                    default:
                        return false;
                }

                bool? Equals(ExpressionSyntax e)
                {
                    return e switch
                    {
                        InvocationExpressionSyntax _ => true,
                        BinaryExpressionSyntax { OperatorToken: { ValueText: "==" } } => true,
                        BinaryExpressionSyntax { OperatorToken: { ValueText: "!=" } } => false,
                        PostfixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: { } operand } => !Equals(operand),
                        _ => null,
                    };
                }

                bool WalkIfStatement(IfStatementSyntax ifStatement)
                {
                    if (!Walk(ifStatement.Statement))
                    {
                        return false;
                    }

                    equals = !equals;
                    if (ifStatement.Else is { Statement: { } })
                    {
                        return Walk(ifStatement.Else.Statement);
                    }

                    return true;
                }
            }
        }
    }
}
