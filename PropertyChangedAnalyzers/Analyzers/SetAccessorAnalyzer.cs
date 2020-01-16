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
            Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
            Descriptors.INPC006UseObjectEqualsForReferenceTypes,
            Descriptors.INPC010GetAndSetSame,
            Descriptors.INPC016NotifyAfterMutation,
            Descriptors.INPC021SetBackingField,
            Descriptors.INPC022EqualToBackingField);

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
                        when Setter.MatchAssign(expression, context.SemanticModel, context.CancellationToken) is { Member: { } member }:
                        if (Property.ShouldNotify(containingProperty, property, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                    containingProperty.Identifier.GetLocation(),
                                    property.Name));
                        }

                        if (ReturnsDifferent(member, context))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC010GetAndSetSame, containingProperty.Identifier.GetLocation()));
                        }

                        break;
                    case { ExpressionBody: { Expression: { } expression } }
                        when Setter.MatchTrySet(expression, context.SemanticModel, context.CancellationToken) is { Member: { } member }:
                        if (ReturnsDifferent(member, context))
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
            EqualState? equalState = null;

            if (Walk(body))
            {
                if (mutation is { Member: { } mutatedMember })
                {
                    if (ReturnsDifferent(mutatedMember, context))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC010GetAndSetSame,
                                body.FirstAncestor<PropertyDeclarationSyntax>()!.Identifier.GetLocation()));
                    }
                    else if (equalState is { MemberAndValue: { Member: { } checkedMember } } &&
                             AreDifferent(checkedMember, mutatedMember, context))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC022EqualToBackingField,
                                checkedMember.GetLocation(),
                                additionalLocations: new[] { mutatedMember.GetLocation() }));
                    }
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC021SetBackingField, body.Parent.GetLocation()));
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

            bool WalkIfStatement(IfStatementSyntax ifStatement)
            {
                if (!Walk(ifStatement.Statement))
                {
                    return false;
                }

                equalState = equalState?.Invert();
                if (ifStatement.Else is { Statement: { } })
                {
                    return Walk(ifStatement.Else.Statement);
                }

                return true;
            }

            void HandleEquality(ExpressionSyntax condition)
            {
                switch (condition)
                {
                    case PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: InvocationExpressionSyntax invocation }:
                        HandleInvocation(invocation);
                        break;
                    case InvocationExpressionSyntax invocation:
                        HandleInvocation(invocation);
                        break;
                    case BinaryExpressionSyntax binary:
                        if (Equality.IsOperatorEquals(binary, out var x, out var y) ||
                            Equality.IsOperatorNotEquals(binary, out x, out y))
                        {
                            if (ShouldUseObjectEquals(x, y))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.INPC006UseObjectEqualsForReferenceTypes,
                                        binary.GetLocation()));
                            }
                            else if (ShouldUseObjectReferenceEquals(x, y))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
                                        binary.GetLocation()));
                            }
                        }

                        break;
                }

                void HandleInvocation(InvocationExpressionSyntax invocation)
                {
                    if (Equality.IsObjectReferenceEquals(invocation, context.SemanticModel, context.CancellationToken, out var x, out var y) &&
                        ShouldUseObjectEquals(x, y))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC006UseObjectEqualsForReferenceTypes,
                                invocation.GetLocation()));
                    }

                    if ((Equality.IsObjectEquals(invocation, context.SemanticModel, context.CancellationToken, out x, out y) ||
                         Equality.IsInstanceEquals(invocation, context.SemanticModel, context.CancellationToken, out x, out y)) &&
                        ShouldUseObjectReferenceEquals(x, y))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
                                invocation.GetLocation()));
                    }
                }

                bool ShouldUseObjectReferenceEquals(ExpressionSyntax x, ExpressionSyntax y)
                {
                    return context.SemanticModel.TryGetType(x, context.CancellationToken, out var xt) &&
                           xt.IsReferenceType &&
                           xt != KnownSymbol.String &&
                           context.SemanticModel.TryGetType(y, context.CancellationToken, out var yt) &&
                           yt.IsReferenceType &&
                           yt != KnownSymbol.String &&
                           xt.Equals(yt) &&
                           !Descriptors.INPC006UseReferenceEqualsForReferenceTypes.IsSuppressed(context.SemanticModel);
                }

                bool ShouldUseObjectEquals(ExpressionSyntax x, ExpressionSyntax y)
                {
                    return context.SemanticModel.TryGetType(x, context.CancellationToken, out var xt) &&
                           xt.IsReferenceType &&
                           xt != KnownSymbol.String &&
                           context.SemanticModel.TryGetType(y, context.CancellationToken, out var yt) &&
                           yt.IsReferenceType &&
                           yt != KnownSymbol.String &&
                           xt.Equals(yt) &&
                           !Descriptors.INPC006UseObjectEqualsForReferenceTypes.IsSuppressed(context.SemanticModel);
                }
            }

            bool Handle(StatementSyntax statement)
            {
                switch (statement)
                {
                    case IfStatementSyntax { Condition: { } condition } ifStatement
                        when Setter.MatchEquals(condition, context.SemanticModel, context.CancellationToken) is { } match:
                        HandleEquality(condition);
                        equalState = EqualState.Create(condition, match);
                        return WalkIfStatement(ifStatement);
                    case IfStatementSyntax { Condition: InvocationExpressionSyntax trySet } ifStatement
                        when Setter.MatchTrySet(trySet, context.SemanticModel, context.CancellationToken) is { } match:
                        mutation = match;
                        equalState = new EqualState(trySet, match, false);
                        return WalkIfStatement(ifStatement);
                    case IfStatementSyntax { Condition: PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: InvocationExpressionSyntax trySet } } ifStatement
                        when Setter.MatchTrySet(trySet, context.SemanticModel, context.CancellationToken) is { } match:
                        mutation = match;
                        equalState = new EqualState(trySet, match, true);
                        return WalkIfStatement(ifStatement);
                    case ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
                        when Setter.MatchAssign(assignment, context.SemanticModel, context.CancellationToken) is { } match:
                        mutation = match;
                        return true;
                    case ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax { Left: IdentifierNameSyntax { Identifier: { ValueText: "_" } }, Right: { } right } }
                        when Setter.MatchTrySet(right, context.SemanticModel, context.CancellationToken) is { } match:
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

                        if (equalState?.IsValueEqualToBacking != false &&
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
            }
        }

        private static bool ReturnsDifferent(ExpressionSyntax assignedMember, SyntaxNodeAnalysisContext context)
        {
            if (context.Node is AccessorDeclarationSyntax { Parent: AccessorListSyntax { Parent: PropertyDeclarationSyntax containingProperty } } &&
                containingProperty.TryGetGetter(out var getter))
            {
                return getter switch
                {
                    { ExpressionBody: { Expression: { } get } }
                    => AreDifferent(get, assignedMember, context),
                    { Body: { Statements: { Count: 0 } statements } }
                    when statements[0] is ReturnStatementSyntax { Expression: { } get }
                    => AreDifferent(get, assignedMember, context),
                    { Body: { } body }
                    when ReturnExpressionsWalker.TryGetSingle(body, out var get)
                    => AreDifferent(get, assignedMember, context),
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

        private struct EqualState
        {
            internal readonly ExpressionSyntax Condition;

            internal readonly BackingMemberAndValue MemberAndValue;

            internal readonly bool? IsValueEqualToBacking;

            internal EqualState(ExpressionSyntax condition, BackingMemberAndValue memberAndValue, bool? isValueEqualToBacking)
            {
                this.Condition = condition;
                this.MemberAndValue = memberAndValue;
                this.IsValueEqualToBacking = isValueEqualToBacking;
            }

            internal static EqualState Create(ExpressionSyntax condition, BackingMemberAndValue memberAndValue)
            {
                return new EqualState(condition, memberAndValue, Equals(condition));

                static bool? Equals(ExpressionSyntax e)
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
            }

            internal EqualState Invert() => new EqualState(
                this.Condition,
                this.MemberAndValue,
                this.IsValueEqualToBacking is { } b ? !b : (bool?)null);
        }
    }
}
