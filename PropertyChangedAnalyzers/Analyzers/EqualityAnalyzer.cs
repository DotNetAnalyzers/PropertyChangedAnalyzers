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
                                IsReferenceType(x, out _) &&
                                IsReferenceType(y, out _))
                            {
                                if (IsInSetter() &&
                                    Descriptors.INPC006UseReferenceEqualsForReferenceTypes.IsSuppressed(context.SemanticModel))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.INPC006UseObjectEqualsForReferenceTypes,
                                            invocation.GetLocation()));
                                }
                            }

                            if ((Gu.Roslyn.AnalyzerExtensions.Equality.IsObjectEquals(invocation, context.SemanticModel, context.CancellationToken, out x, out y) ||
                                 Gu.Roslyn.AnalyzerExtensions.Equality.IsInstanceEquals(invocation, context.SemanticModel, context.CancellationToken, out x, out y)) &&
                                IsReferenceType(x, out var typeX) &&
                                IsReferenceType(y, out var typeY) &&
                                Equals(typeX, typeY))
                            {
                                if (IsInSetter() &&
                                    Descriptors.INPC006UseObjectEqualsForReferenceTypes.IsSuppressed(context.SemanticModel))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
                                            invocation.GetLocation()));
                                }
                            }

                            break;
                        }

                    case BinaryExpressionSyntax binary:
                        {
                            if ((Gu.Roslyn.AnalyzerExtensions.Equality.IsOperatorEquals(binary, out var x, out var y) ||
                                 Gu.Roslyn.AnalyzerExtensions.Equality.IsOperatorNotEquals(binary, out x, out y)) &&
                                IsReferenceType(x, out var xType) &&
                                xType != KnownSymbol.String &&
                                IsReferenceType(y, out _))
                            {
                                if (IsInSetter())
                                {
                                    if (Descriptors.INPC006UseReferenceEqualsForReferenceTypes.IsSuppressed(context.SemanticModel))
                                    {
                                        context.ReportDiagnostic(
                                            Diagnostic.Create(
                                                Descriptors.INPC006UseObjectEqualsForReferenceTypes,
                                                binary.GetLocation()));
                                    }
                                    else if (Descriptors.INPC006UseObjectEqualsForReferenceTypes.IsSuppressed(context.SemanticModel))
                                    {
                                        context.ReportDiagnostic(
                                            Diagnostic.Create(
                                                Descriptors.INPC006UseReferenceEqualsForReferenceTypes,
                                                binary.GetLocation()));
                                    }
                                }
                            }

                            break;
                        }
                }
            }

            bool IsReferenceType(ExpressionSyntax candidate, out ITypeSymbol type)
            {
                return context.SemanticModel.TryGetType(candidate, context.CancellationToken, out type) &&
                       type.IsReferenceType;
            }

            bool IsInSetter()
            {
                return context.Node.TryFirstAncestor(out AccessorDeclarationSyntax accessor) &&
                       accessor.IsKind(SyntaxKind.SetAccessorDeclaration);
            }
        }
    }
}
