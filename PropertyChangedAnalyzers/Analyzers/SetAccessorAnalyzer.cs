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
            Descriptors.INPC010GetAndSetSame,
            Descriptors.INPC016NotifyAfterMutation,
            Descriptors.INPC021SetBackingField);

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
                    case { ExpressionBody: { Expression: { } expression } }
                        when Setter.MatchAssign(expression, context.SemanticModel, context.CancellationToken) is { } match:
                        if (Property.ShouldNotify(containingProperty, property, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                    containingProperty.Identifier.GetLocation(),
                                    property.Name));
                        }

                        if (ReturnsDifferent(match, context))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC010GetAndSetSame, containingProperty.Identifier.GetLocation()));
                        }

                        break;
                    case { ExpressionBody: { Expression: { } expression } }
                        when Setter.MatchTrySet(expression, context.SemanticModel, context.CancellationToken) is { } match:
                        if (ReturnsDifferent(match, context))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC010GetAndSetSame, containingProperty.Identifier.GetLocation()));
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
            BackingMemberAndValue? mutation = null;
            bool? equals = null;

            if (Walk(body))
            {
                if (mutation is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC021SetBackingField, body.Parent.GetLocation()));
                }
                else if (ReturnsDifferent(mutation.Value, context))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.INPC010GetAndSetSame,
                            body.FirstAncestor<PropertyDeclarationSyntax>()!.Identifier.GetLocation()));
                }
            }

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
                        when Setter.MatchEquals(condition, context.SemanticModel, context.CancellationToken) is { }:
                        equals = Equals(condition);
                        return WalkIfStatement(ifStatement);
                    case IfStatementSyntax { Condition: InvocationExpressionSyntax trySet } ifStatement
                        when Setter.MatchTrySet(trySet, context.SemanticModel, context.CancellationToken) is { } match:
                        mutation = match;
                        equals = false;
                        return WalkIfStatement(ifStatement);
                    case IfStatementSyntax { Condition: PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: InvocationExpressionSyntax trySet } } ifStatement
                        when Setter.MatchTrySet(trySet, context.SemanticModel, context.CancellationToken) is { } match:
                        mutation = match;
                        equals = true;
                        return WalkIfStatement(ifStatement);
                    case ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
                        when Setter.MatchAssign(assignment, context.SemanticModel, context.CancellationToken) is { } match:
                        mutation = match;
                        return true;
                    case ExpressionStatementSyntax { Expression: InvocationExpressionSyntax trySet }
                        when Setter.MatchTrySet(trySet, context.SemanticModel, context.CancellationToken) is { } match:
                        mutation = match;
                        return true;
                    case ExpressionStatementSyntax { Expression: ConditionalAccessExpressionSyntax { WhenNotNull: InvocationExpressionSyntax conditionalInvoke } }
                        when PropertyChangedEvent.IsInvoke(conditionalInvoke, context.SemanticModel, context.CancellationToken):
                    case ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invoke }
                        when PropertyChangedEvent.IsInvoke(invoke, context.SemanticModel, context.CancellationToken):
                    case ExpressionStatementSyntax { Expression: InvocationExpressionSyntax onPropertyChanged }
                        when OnPropertyChanged.Match(onPropertyChanged, context.SemanticModel, context.CancellationToken) is { }:
                        if (mutation is null)
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
                        PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: { } operand } => !Equals(operand),
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

        private static bool ReturnsDifferent(BackingMemberAndValue andValue, SyntaxNodeAnalysisContext context)
        {
            if (context.Node is AccessorDeclarationSyntax { Parent: AccessorListSyntax { Parent: PropertyDeclarationSyntax containingProperty } } &&
                containingProperty.TryGetGetter(out var getter))
            {
                return getter switch
                {
                    { ExpressionBody: { Expression: { } get } }
                    => AreDifferent(get, andValue.Member, context),
                    { Body: { Statements: { Count: 0 } statements } }
                    when statements[0] is ReturnStatementSyntax { Expression: { } get }
                    => AreDifferent(get, andValue.Member, context),
                    { Body: { } body }
                    when ReturnExpressionsWalker.TryGetSingle(body, out var get)
                    => AreDifferent(get, andValue.Member, context),
                    _ => false
                };
            }

            return false;
        }

        private static bool AreDifferent(ExpressionSyntax get, ExpressionSyntax set, SyntaxNodeAnalysisContext context)
        {
            using var getPath = MemberPath.Get(get);
            using var setPath = MemberPath.Get(set);

            if (MemberPath.Equals(getPath, setPath))
            {
                return false;
            }

            if (Member(getPath, 0) is { } getField &&
                Member(setPath, 0) is { } setField)
            {
                if (getField.Equals(setField))
                {
                    if (Member(getPath, 1) is { } getField1 &&
                        Member(setPath, 1) is { } setField1)
                    {
                        return !getField1.Equals(setField1);
                    }

                    return false;
                }

                return true;
            }

            return false;

            ISymbol? Member(MemberPath.PathWalker path, int index)
            {
                if (path.Tokens.TryElementAt(index, out var token))
                {
                    return context.SemanticModel.GetSymbolSafe(token.Parent, context.CancellationToken) switch
                    {
                        IFieldSymbol field => field,
                        IPropertySymbol property
                        when property.IsAutoProperty()
                        => property,
                        _ => null,
                    };
                }

                return null;
            }
        }
    }
}
