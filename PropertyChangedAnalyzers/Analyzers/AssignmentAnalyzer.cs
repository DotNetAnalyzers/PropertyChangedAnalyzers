namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class AssignmentAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC014SetBackingFieldInConstructor);

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
                ShouldSetBackingField(assignment, context) is {} fieldAccess)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.INPC014SetBackingFieldInConstructor,
                        assignment.GetLocation(),
                        additionalLocations: new[] { fieldAccess.GetLocation() }));
            }
        }

        private static ExpressionSyntax? ShouldSetBackingField(AssignmentExpressionSyntax assignment, SyntaxNodeAnalysisContext context)
        {
            if (context.ContainingSymbol is IMethodSymbol { IsStatic: false, MethodKind: MethodKind
                    .Constructor } ctor &&
                Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                !assignment.TryFirstAncestor<AnonymousFunctionExpressionSyntax>(out _) &&
                !assignment.TryFirstAncestor<LocalFunctionStatementSyntax>(out _) &&
                propertyDeclaration.TryGetSetter(out var setter))
            {
                switch (setter)
                {
                    case { ExpressionBody: { } }:
                        return FindAssignedField();
                    case { Body: { Statements: { } statements } }:
                        foreach (var statement in statements)
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
                            return null;
                        }

                        return FindAssignedField();
                    default:
                        return null;
                }
            }

            return null;

            ExpressionSyntax? FindAssignedField()
            {
                return Setter.FindSingleMutated(setter, context.SemanticModel, context.CancellationToken) is {} backingField &&
                       MemberPath.TrySingle(backingField, out var single) &&
                       context.SemanticModel.TryGetSymbol(single, context.CancellationToken, out IFieldSymbol? field) &&
                       Equals(ctor.ContainingType, field.ContainingType)
                    ? backingField
                    : null;
            }

            bool IsWhiteListedExpression(ExpressionStatementSyntax candidate)
            {
                return candidate.Expression switch
                {
                    InvocationExpressionSyntax invocation
                    => OnPropertyChanged.IsMatch(invocation, context.SemanticModel, context.CancellationToken, out _) != AnalysisResult.No ||
                       TrySet.IsMatch(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No,
                    AssignmentExpressionSyntax candidateAssignment
                    => Setter.IsMutation(candidateAssignment, context.SemanticModel, context.CancellationToken, out _, out _),
                    _ => false,
                };
            }

            bool IsWhiteListedStatement(StatementSyntax candidate)
            {
                return candidate switch
                {
                    ReturnStatementSyntax _ => true,
                    ExpressionStatementSyntax expressionStatement => IsWhiteListedExpression(expressionStatement),
                    _ => false,
                };
            }

            bool IsWhiteListedIfStatement(IfStatementSyntax ifStatement)
            {
                return IsWhiteListedBranch(ifStatement.Statement) &&
                       IsWhiteListedBranch(ifStatement.Else?.Statement);

                bool IsWhiteListedBranch(StatementSyntax? branch)
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
