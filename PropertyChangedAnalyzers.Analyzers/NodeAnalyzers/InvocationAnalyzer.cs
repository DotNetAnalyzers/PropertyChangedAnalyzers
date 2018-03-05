namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InvocationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            INPC009DontRaiseChangeForMissingProperty.Descriptor,
            INPC016NotifyAfterUpdate.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation)
            {
                if (PropertyChanged.IsPropertyChangedInvoker(invocation, context.SemanticModel, context.CancellationToken))
                {
                    if ((invocation.ArgumentList == null ||
                         invocation.ArgumentList.Arguments.Count == 0) &&
                        invocation.FirstAncestor<AccessorDeclarationSyntax>() == null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, invocation.GetLocation()));
                    }

                    if (invocation.FirstAncestor<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax setter &&
                        setter.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                        Property.TrySingleAssignmentInSetter(setter, out var assignment))
                    {
                        if (!AreInSameBlock(assignment, invocation) ||
                            assignment.SpanStart > invocation.SpanStart)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC016NotifyAfterUpdate.Descriptor, invocation.GetLocation()));
                        }
                    }
                }
            }
        }

        private static bool AreInSameBlock(SyntaxNode node1, SyntaxNode node2)
        {
            if (node1?.FirstAncestor<BlockSyntax>() is BlockSyntax block1 &&
                node2?.FirstAncestor<BlockSyntax>() is BlockSyntax block2)
            {
                return block1 == block2 ||
                       block1.Contains(block2) ||
                       block2.Contains(block1);
            }

            return false;
        }
    }
}
