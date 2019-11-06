namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class AssignmentAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC014SetBackingFieldInConstructor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.SimpleAssignmentExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AssignmentExpressionSyntax assignment &&
                ShouldSetBackingField(assignment, context, out var fieldAccess))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.INPC014SetBackingFieldInConstructor,
                        assignment.GetLocation(),
                        additionalLocations: new[] { fieldAccess.GetLocation() }));
            }
        }

        private static bool ShouldSetBackingField(AssignmentExpressionSyntax assignment, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out ExpressionSyntax? fieldAccess)
        {
            fieldAccess = null;
            return context.ContainingSymbol is IMethodSymbol { IsStatic: false, MethodKind: MethodKind.Constructor } ctor &&
                   assignment.Parent is ExpressionStatementSyntax assignmentStatement &&
                   assignmentStatement.TryFirstAncestor(out ConstructorDeclarationSyntax? constructor) &&
                   constructor.Body is { Statements: { } statements } &&
                   statements.Contains(assignmentStatement) &&
                   Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                   propertyDeclaration.TryGetSetter(out var setter) &&
                   IsSimple(out fieldAccess);

            bool IsSimple(out ExpressionSyntax backing)
            {
                if (setter.ExpressionBody != null)
                {
                    return IsAssigningField(out backing);
                }

                if (setter.Body is BlockSyntax block)
                {
                    foreach (var statement in block.Statements)
                    {
                        if (IsWhiteListedStatement(statement))
                        {
                            continue;
                        }

                        if (statement is IfStatementSyntax ifStatement &&
                            IsWhiteListedIfStatement(ifStatement))
                        {
                            continue;
                        }

                        // If there is for example validation or side effects we don't suggest setting the field.
                        backing = null;
                        return false;
                    }

                    return IsAssigningField(out backing);
                }

                backing = null;
                return false;
            }

            bool IsAssigningField(out ExpressionSyntax backingField)
            {
                return Setter.TryFindSingleMutation(setter, context.SemanticModel, context.CancellationToken, out backingField) &&
                       MemberPath.TrySingle(backingField, out var single) &&
                       context.SemanticModel.TryGetSymbol(single, context.CancellationToken, out IFieldSymbol? field) &&
                       Equals(ctor.ContainingType, field.ContainingType);
            }

            bool IsWhiteListedExpression(ExpressionStatementSyntax candidate)
            {
                switch (candidate.Expression)
                {
                    case InvocationExpressionSyntax invocation
                        when OnPropertyChanged.IsMatch(invocation, context.SemanticModel, context.CancellationToken, out _) != AnalysisResult.No ||
                             TrySet.IsMatch(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No:
                        return true;
                    case AssignmentExpressionSyntax candidateAssignment
                        when Setter.IsMutation(candidateAssignment, context.SemanticModel, context.CancellationToken, out _, out _):
                        return true;
                    default:
                        return false;
                }
            }

            static bool IsWhiteListedStatement(StatementSyntax candidate)
            {
                switch (candidate)
                {
                    case ReturnStatementSyntax _:
                        return true;
                    case ExpressionStatementSyntax expressionStatement
                        when IsWhiteListedExpression(expressionStatement):
                        return true;
                    default:
                        // If there is for example validation or side effects we don't suggest setting the field.
                        return false;
                }
            }

            static bool IsWhiteListedIfStatement(IfStatementSyntax ifStatement)
            {
                return IsWhiteListedBranch(ifStatement.Statement) &&
                       IsWhiteListedBranch(ifStatement.Else?.Statement);

                static bool IsWhiteListedBranch(StatementSyntax? branch)
                {
                    switch (branch)
                    {
                        case BlockSyntax branchBlock:
                            foreach (var branchStatement in branchBlock.Statements)
                            {
                                if (!IsWhiteListedStatement(branchStatement))
                                {
                                    return false;
                                }
                            }

                            return true;
                        case { } statement:
                            return IsWhiteListedStatement(statement);
                        case null:
                            return true;
                    }
                }
            }
        }
    }
}
