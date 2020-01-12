namespace PropertyChangedAnalyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TrySet
    {
        internal static bool CanCreateInvocation(IMethodSymbol candidate, [NotNullWhen(true)] out IParameterSymbol? nameParameter)
        {
            nameParameter = null;
            return candidate is { IsGenericMethod: true } &&
                   candidate.TypeParameters.TrySingle(out var typeParameter) &&
                   candidate.Parameters.Length > 2 &&
                   candidate.Parameters[0].RefKind == RefKind.Ref &&
                   candidate.Parameters[0].Type.Equals(typeParameter) &&
                   candidate.Parameters[1].RefKind == RefKind.None &&
                   candidate.Parameters[1].Type.Equals(typeParameter) &&
                   candidate.Parameters.TrySingle(x => x is { Type: { SpecialType: SpecialType.System_String } }, out nameParameter) &&
                   RestAreOptional();

            bool RestAreOptional()
            {
                for (var i = 2; i < candidate.Parameters.Length; i++)
                {
                    var parameter = candidate.Parameters[i];
                    if (parameter.Type != KnownSymbol.String &&
                        !parameter.IsOptional)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal static bool TryFind(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return type.TryFindFirstMethodRecursive(x => TrySet.CanCreateInvocation(x, out _) && IsMatch(x, semanticModel, cancellationToken) != AnalysisResult.No, out method);
        }

        internal static AnalysisResult IsMatch(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (candidate.FirstAncestor<TypeDeclarationSyntax>() is { } containingTypeDeclaration &&
                semanticModel.TryGetNamedType(containingTypeDeclaration, cancellationToken, out var containingType))
            {
                using var recursion = Recursion.Borrow(containingType, semanticModel, cancellationToken);
                return IsMatch(candidate, recursion);
            }

            return AnalysisResult.No;
        }

        internal static AnalysisResult IsMatch(IMethodSymbol candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (candidate.ContainingType is { } containingType)
            {
                using var recursion = Recursion.Borrow(containingType, semanticModel, cancellationToken);
                return IsMatch(candidate, recursion);
            }

            return AnalysisResult.No;
        }

        internal static TrySetMatch? Match(IMethodSymbol candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var result = IsMatch(candidate, semanticModel, cancellationToken);
            if (result == AnalysisResult.No)
            {
                return null;
            }

            if (candidate is { Parameters: { } parameters } &&
                parameters.TrySingle(x => x is { RefKind: RefKind.Ref, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out var field) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out var value) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { SpecialType: SpecialType.System_String } } }, out var name))
            {
                return new TrySetMatch(result, field, value, name);
            }

            throw new InvalidOperationException("Bug in PropertyChangedAnalyzers. Could not get parameters.");
        }

        private static AnalysisResult IsMatch(InvocationExpressionSyntax candidate, Recursion recursion)
        {
            if (candidate?.ArgumentList is null ||
                candidate.ArgumentList.Arguments.Count < 2 ||
                !candidate.ArgumentList.Arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword), out _) ||
                !candidate.IsPotentialThisOrBase())
            {
                return AnalysisResult.No;
            }

            if (recursion.Target(candidate) is { Symbol: IMethodSymbol target })
            {
                return IsMatch(target, recursion);
            }

            return AnalysisResult.No;
        }

        private static AnalysisResult IsMatch(IMethodSymbol candidate, Recursion recursion)
        {
            if (candidate is { MethodKind: MethodKind.Ordinary, ReturnType: { SpecialType: SpecialType.System_Boolean }, IsGenericMethod: true, TypeParameters: { Length: 1 }, Parameters: { } parameters } &&
                parameters.TrySingle(x => x is { RefKind: RefKind.Ref, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out var field) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out var value) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { SpecialType: SpecialType.System_String } } }, out _) &&
                ShouldCheck())
            {
                if (candidate.TrySingleMethodDeclaration(recursion.CancellationToken, out var methodDeclaration))
                {
                    using (var walker = InvocationWalker.Borrow(methodDeclaration))
                    {
                        if (!walker.Invocations.TrySingle(x => Notifies(x), out _))
                        {
                            var result = AnalysisResult.No;
                            foreach (var invocation in walker.Invocations)
                            {
                                switch (IsMatch(invocation, recursion))
                                {
                                    case AnalysisResult.No:
                                        break;
                                    case AnalysisResult.Yes:
                                        return AnalysisResult.Yes;
                                    case AnalysisResult.Maybe:
                                        result = AnalysisResult.Maybe;
                                        break;
                                    default:
                                        throw new InvalidOperationException("Unknown AnalysisResult");
                                }
                            }

                            return result;
                        }

                        bool Notifies(InvocationExpressionSyntax x)
                        {
                            return OnPropertyChanged.Match(x, recursion.SemanticModel, recursion.CancellationToken) is { } ||
                                   PropertyChangedEvent.IsInvoke(x, recursion.SemanticModel, recursion.CancellationToken);
                        }
                    }

                    using (var walker = AssignmentWalker.Borrow(methodDeclaration))
                    {
                        if (!walker.Assignments.TrySingle(x => Assigns(x), out _))
                        {
                            return AnalysisResult.No;
                        }

                        bool Assigns(AssignmentExpressionSyntax x)
                        {
                            return x.Left is IdentifierNameSyntax left &&
                                   left.Identifier.ValueText == field!.Name &&
                                   x.Right is IdentifierNameSyntax right &&
                                   right.Identifier.ValueText == value!.Name;
                        }
                    }

                    return AnalysisResult.Yes;
                }

                if (candidate == KnownSymbol.MvvmLightViewModelBase.Set ||
                    candidate == KnownSymbol.MvvmLightObservableObject.Set ||
                    candidate == KnownSymbol.CaliburnMicroPropertyChangedBase.Set ||
                    candidate == KnownSymbol.StyletPropertyChangedBase.SetAndNotify ||
                    candidate == KnownSymbol.MvvmCrossMvxNotifyPropertyChanged.SetProperty ||
                    candidate == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.SetProperty ||
                    candidate == KnownSymbol.MicrosoftPracticesPrismMvvmBindableBase.SetProperty)
                {
                    return AnalysisResult.Yes;
                }

                return candidate.Parameters.Length == 3
                    ? AnalysisResult.Maybe
                    : AnalysisResult.No;
            }

            return AnalysisResult.No;

            bool ShouldCheck()
            {
                return candidate switch
                {
                    { IsStatic: true } => PropertyChangedEvent.Find(candidate.ContainingType) is { IsStatic: true },
                    { IsStatic: false } => candidate.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, recursion.SemanticModel.Compilation),
                    _ => false, // never getting here, candidate never null.
                };
            }
        }
    }
}
