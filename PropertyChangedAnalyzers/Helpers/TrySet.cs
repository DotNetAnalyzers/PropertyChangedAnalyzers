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
                   candidate.Parameters.TrySingle<IParameterSymbol>(x => x.Type == KnownSymbol.String, out nameParameter) &&
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

        internal static bool IsMatchWhenInSetter(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ArgumentSyntax? field, [NotNullWhen(true)] out ArgumentSyntax? value)
        {
            field = null;
            value = null;
            return candidate is { ArgumentList: { Arguments: { } arguments } } &&
                   IsMatch(candidate, semanticModel, cancellationToken) != AnalysisResult.No &&
                   arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefExpression), out field) &&
                   arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.None) && x.Expression is IdentifierNameSyntax { Identifier: { ValueText: "value" } }, out value);
        }

        internal static AnalysisResult IsMatch(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<IMethodSymbol>? visited = null)
        {
            if (candidate?.ArgumentList is null ||
                candidate.ArgumentList.Arguments.Count < 2 ||
                !candidate.ArgumentList.Arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword), out _) ||
                !candidate.IsPotentialThisOrBase())
            {
                return AnalysisResult.No;
            }

            return IsMatch(
                semanticModel.GetSymbolSafe(candidate, cancellationToken),
                semanticModel,
                cancellationToken,
                visited);
        }

        internal static AnalysisResult IsMatch(IMethodSymbol candidate, SemanticModel semanticModel, CancellationToken cancellationToken, out IParameterSymbol field, out IParameterSymbol value, out IParameterSymbol name)
        {
            var result = IsMatch(candidate, semanticModel, cancellationToken);
            if (result == AnalysisResult.No)
            {
                field = null;
                value = null;
                name = null;
                return AnalysisResult.No;
            }

            if (candidate is { Parameters: { } parameters } &&
                parameters.TrySingle(x => x is { RefKind: RefKind.Ref, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out field) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out value) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { SpecialType: SpecialType.System_String } } }, out name))
            {
                return result;
            }

            throw new InvalidOperationException("Bug in PropertyChangedAnalyzers. Could not get parameters.");
        }

        internal static AnalysisResult IsMatch(IMethodSymbol candidate, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<IMethodSymbol>? visited = null)
        {
            if (visited?.Add(candidate) == false)
            {
                return AnalysisResult.No;
            }

            if (candidate is { MethodKind: MethodKind.Ordinary, ReturnType: { SpecialType: SpecialType.System_Boolean }, IsGenericMethod: true, TypeParameters: { Length: 1 }, Parameters: { } parameters } &&
                parameters.TrySingle(x => x is { RefKind: RefKind.Ref, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out _) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { TypeKind: TypeKind.TypeParameter } } }, out _) &&
                parameters.TrySingle(x => x is { RefKind: RefKind.None, OriginalDefinition: { Type: { SpecialType: SpecialType.System_String } } }, out _) &&
                ShouldCheck())
            {
                if (candidate.TrySingleMethodDeclaration(cancellationToken, out var methodDeclaration))
                {
                    using (var walker = InvocationWalker.Borrow(methodDeclaration))
                    {
                        if (!walker.Invocations.TrySingle(
                            x => OnPropertyChanged.IsMatch(x, semanticModel, cancellationToken) != AnalysisResult.No ||
                                 PropertyChangedEvent.IsInvoke(x, semanticModel, cancellationToken),
                            out _))
                        {
                            using var set = visited.IncrementUsage();
                            var result = AnalysisResult.No;
                            foreach (var invocation in walker.Invocations)
                            {
                                switch (IsMatch(invocation, semanticModel, cancellationToken, set))
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
                    }

                    using (var walker = AssignmentWalker.Borrow(methodDeclaration))
                    {
                        if (!walker.Assignments.TrySingle(
                            x => semanticModel.GetSymbolSafe(x.Left, cancellationToken)?.Name == candidate.Parameters[0].Name &&
                                 semanticModel.GetSymbolSafe(x.Right, cancellationToken)?.Name == candidate.Parameters[1].Name,
                            out _))
                        {
                            return AnalysisResult.No;
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

                return candidate.Parameters.Length == 3 ? AnalysisResult.Maybe : AnalysisResult.No;
            }

            return AnalysisResult.No;

            bool ShouldCheck()
            {
                return candidate switch
                {
                    { IsStatic: true } => PropertyChangedEvent.TryFind(candidate.ContainingType, out _),
                    { IsStatic: false } => candidate.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, semanticModel.Compilation),
                };
            }
        }
    }
}
