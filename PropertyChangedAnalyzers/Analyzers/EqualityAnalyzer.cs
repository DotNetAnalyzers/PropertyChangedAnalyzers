namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class EqualityAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
            Descriptors.INPC006UseObjectEqualsForReferenceTypes);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.InvocationExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis())
            {
                switch (context.Node)
                {
                    case InvocationExpressionSyntax invocation:
                        {
                            if (Gu.Roslyn.AnalyzerExtensions.Equality.IsObjectReferenceEquals(invocation, context.SemanticModel, context.CancellationToken, out var x, out var y) &&
                                UseEquals(x, y))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.INPC006UseObjectEqualsForReferenceTypes,
                                        invocation.GetLocation()));
                            }

                            if ((Gu.Roslyn.AnalyzerExtensions.Equality.IsObjectEquals(invocation, context.SemanticModel, context.CancellationToken, out x, out y) ||
                                 Gu.Roslyn.AnalyzerExtensions.Equality.IsInstanceEquals(invocation, context.SemanticModel, context.CancellationToken, out x, out y)) &&
                                UseReferenceEquals(x, y))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
                                        invocation.GetLocation()));
                            }

                            break;
                        }

                    case BinaryExpressionSyntax binary:
                        {
                            if (Gu.Roslyn.AnalyzerExtensions.Equality.IsOperatorEquals(binary, out var x, out var y) ||
                                Gu.Roslyn.AnalyzerExtensions.Equality.IsOperatorNotEquals(binary, out x, out y))
                            {
                                if (UseEquals(x, y))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.INPC006UseObjectEqualsForReferenceTypes,
                                            binary.GetLocation()));
                                }
                                else if (UseReferenceEquals(x, y))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
                                            binary.GetLocation()));
                                }
                            }

                            break;
                        }
                }
            }

            bool IsInSetter()
            {
                return context.Node.TryFirstAncestor(out AccessorDeclarationSyntax accessor) &&
                       accessor.IsKind(SyntaxKind.SetAccessorDeclaration);
            }

            bool UseReferenceEquals(ExpressionSyntax x, ExpressionSyntax y)
            {
                return context.SemanticModel.TryGetType(x, context.CancellationToken, out var xt) &&
                       xt.IsReferenceType &&
                       xt != KnownSymbol.String &&
                       context.SemanticModel.TryGetType(y, context.CancellationToken, out var yt) &&
                       yt.IsReferenceType &&
                       yt != KnownSymbol.String &&
                       xt.Equals(yt) &&
                       !Descriptors.INPC006UseReferenceEqualsForReferenceTypes.IsSuppressed(context.SemanticModel) &&
                       IsInSetter();
            }

            bool UseEquals(ExpressionSyntax x, ExpressionSyntax y)
            {
                return context.SemanticModel.TryGetType(x, context.CancellationToken, out var xt) &&
                       xt.IsReferenceType &&
                       xt != KnownSymbol.String &&
                       context.SemanticModel.TryGetType(y, context.CancellationToken, out var yt) &&
                       yt.IsReferenceType &&
                       yt != KnownSymbol.String &&
                       xt.Equals(yt) &&
                       !Descriptors.INPC006UseObjectEqualsForReferenceTypes.IsSuppressed(context.SemanticModel) &&
                       IsInSetter();
            }
        }
    }
}
