namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
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
                   Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                   !assignment.TryFirstAncestor<AnonymousFunctionExpressionSyntax>(out _) &&
                   !assignment.TryFirstAncestor<LocalFunctionStatementSyntax>(out _) &&
                   propertyDeclaration.TryGetSetter(out var setter) &&
                   IsSimple(out fieldAccess);

            bool IsSimple(out ExpressionSyntax? backing)
            {
                switch (setter)
                {
                    case { ExpressionBody: { } }:
                        return IsAssigningField(out backing);
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
                            backing = null;
                            return false;
                        }

                        return IsAssigningField(out backing);
                    default:
                        backing = null;
                        return false;
                }
            }

            bool IsAssigningField(out ExpressionSyntax? backingField)
            {
#pragma warning disable CS8604 // Possible null reference argument. CompilerBug
                return Setter.FindSingleMutated(setter, context.SemanticModel, context.CancellationToken, out backingField) &&
#pragma warning restore CS8604 // Possible null reference argument.
                       MemberPath.TrySingle(backingField, out var single) &&
                       context.SemanticModel.TryGetSymbol(single, context.CancellationToken, out IFieldSymbol? field) &&
                       Equals(ctor.ContainingType, field.ContainingType);
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
