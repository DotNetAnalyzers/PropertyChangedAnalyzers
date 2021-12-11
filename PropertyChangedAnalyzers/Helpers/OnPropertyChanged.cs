namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct OnPropertyChanged
    {
        internal readonly AnalysisResult AnalysisResult;
        internal readonly IParameterSymbol Name;

        internal OnPropertyChanged(AnalysisResult analysisResult, IParameterSymbol name)
        {
            this.AnalysisResult = analysisResult;
            this.Name = name;
        }

        internal static IMethodSymbol? Find(IEventSymbol propertyChanged, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            IMethodSymbol? match = null;
            using var recursion = Recursion.Borrow(propertyChanged.ContainingType, semanticModel, cancellationToken);
            while (propertyChanged != null)
            {
                foreach (var member in propertyChanged.ContainingType.GetMembers())
                {
                    if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } candidate &&
                        candidate.IsStatic == propertyChanged.IsStatic)
                    {
                        if (!TypeSymbolComparer.Equal(candidate.ContainingType, recursion.ContainingType) &&
                            candidate.DeclaredAccessibility == Accessibility.Private)
                        {
                            continue;
                        }

                        switch (IsMatch(candidate, recursion))
                        {
                            case AnalysisResult.No:
                                continue;
                            case AnalysisResult.Yes:
                            case AnalysisResult.Maybe:
                                if (candidate.Parameters.TrySingle(out var parameter) &&
                                    parameter.Type is { SpecialType: SpecialType.System_String })
                                {
                                    return candidate;
                                }
                                else
                                {
                                    match = candidate;
                                    break;
                                }

                            default:
                                throw new InvalidOperationException("Unknown AnalysisResult");
                        }
                    }
                }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                propertyChanged = propertyChanged.OverriddenEvent;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            return match;
        }

        internal static IMethodSymbol? Find(INamedTypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return PropertyChanged.Find(type) is { } propertyChanged
                ? Find(propertyChanged, semanticModel, cancellationToken)
                : null;
        }

        internal static OnPropertyChanged? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation.ArgumentList.Arguments.Count > 1 ||
                !invocation.IsPotentialReturnVoid() ||
                !invocation.IsPotentialThisOrBase())
            {
                return null;
            }

            if (invocation.TryFirstAncestor(out ClassDeclarationSyntax? containingClass))
            {
                if (containingClass.BaseList?.Types is null ||
                    containingClass.BaseList.Types.Count == 0)
                {
                    return null;
                }

                if (semanticModel.TryGetSymbol(invocation, cancellationToken, out var method))
                {
                    return Match(method, semanticModel, cancellationToken);
                }
            }

            return null;
        }

        internal static OnPropertyChanged? Match(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using var recursion = Recursion.Borrow(method.ContainingType, semanticModel, cancellationToken);
            var result = IsMatch(method, recursion);
            if (result != AnalysisResult.No &&
                method.Parameters.TrySingle(out var parameter))
            {
                return new OnPropertyChanged(result, parameter);
            }

            return null;
        }

        private static AnalysisResult IsMatch(IMethodSymbol method, Recursion recursion)
        {
            if (!IsPotentialMatch(method, recursion.SemanticModel.Compilation))
            {
                return AnalysisResult.No;
            }

            // not using known symbol here as both jetbrains & mvvm cross defines a NotifyPropertyChangedInvocatorAttribute
            if (method.GetAttributes().TryFirst(x => x.AttributeClass?.Name == "NotifyPropertyChangedInvocatorAttribute", out _))
            {
                return AnalysisResult.Yes;
            }

            var result = AnalysisResult.No;
            if (method.Parameters.TrySingle(out var parameter) &&
                method.TrySingleDeclaration(recursion.CancellationToken, out MethodDeclarationSyntax? declaration))
            {
                using var walker = InvocationWalker.Borrow(declaration);
                foreach (var invocation in walker.Invocations)
                {
                    if (PropertyChanged.Invoke.Match(invocation, recursion.SemanticModel, recursion.CancellationToken) is { EventArgument: { } argument })
                    {
                        if (argument.Expression is IdentifierNameSyntax identifierName &&
                            identifierName.Identifier.ValueText == parameter.Name)
                        {
                            return AnalysisResult.Yes;
                        }

                        if (PropertyChangedEventArgs.IsCreatedWith(argument.Expression, parameter, recursion.SemanticModel, recursion.CancellationToken))
                        {
                            return AnalysisResult.Yes;
                        }
                    }
                    else if (invocation is { ArgumentList: { Arguments: { Count: 1 } arguments } } &&
                             arguments[0] is { Expression: { } expression } &&
                             invocation.IsPotentialThisOrBase())
                    {
                        if (PropertyChangedEventArgs.IsCreatedWith(expression, parameter, recursion.SemanticModel, recursion.CancellationToken) ||
                            IdentifierNameWalker.Contains(expression, parameter, recursion.SemanticModel, recursion.CancellationToken))
                        {
                            if (recursion.Target(invocation) is { Symbol: IMethodSymbol invokedMethod })
                            {
                                switch (IsMatch(invokedMethod, recursion))
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
                        }
                    }
                }
            }
            else
            {
                if (method == KnownSymbol.MvvmLightViewModelBase.RaisePropertyChanged ||
                    method == KnownSymbol.MvvmLightViewModelBase.RaisePropertyChangedOfT ||
                    method == KnownSymbol.MvvmLightObservableObject.RaisePropertyChanged ||
                    method == KnownSymbol.MvvmLightObservableObject.RaisePropertyChangedOfT ||
                    method == KnownSymbol.CaliburnMicroPropertyChangedBase.NotifyOfPropertyChange ||
                    method == KnownSymbol.CaliburnMicroPropertyChangedBase.NotifyOfPropertyChangeOfT ||
                    method == KnownSymbol.StyletPropertyChangedBase.NotifyOfPropertyChange ||
                    method == KnownSymbol.StyletPropertyChangedBase.NotifyOfPropertyChangeOfT ||
                    method == KnownSymbol.MvvmCrossMvxNotifyPropertyChanged.RaisePropertyChanged ||
                    method == KnownSymbol.MvvmCrossMvxNotifyPropertyChanged.RaisePropertyChangedOfT ||
                    method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChanged ||
                    method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChangedOfT ||
                    method == KnownSymbol.MicrosoftPracticesPrismMvvmBindableBase.OnPropertyChanged ||
                    method == KnownSymbol.MicrosoftPracticesPrismMvvmBindableBase.OnPropertyChangedOfT)
                {
                    return AnalysisResult.Yes;
                }

                if (parameter != null &&
                    parameter.Type.IsEither(KnownSymbol.String, KnownSymbol.PropertyChangedEventArgs))
                {
                    if (method.Name.Contains("PropertyChanged"))
                    {
                        // A bit speculative here
                        // for handling the case when inheriting a ViewModelBase class from a binary reference.
                        return AnalysisResult.Maybe;
                    }
                }

                return AnalysisResult.No;
            }

            return result;
        }

        private static bool IsPotentialMatch(IMethodSymbol method, Compilation compilation)
        {
            if (method == KnownSymbol.MvvmCrossMvxNotifyPropertyChanged.RaisePropertyChanged ||
                method == KnownSymbol.MvvmCrossMvxNotifyPropertyChanged.RaisePropertyChangedOfT ||
                method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChanged ||
                method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChangedOfT)
            {
                // They changed return type to task, special casing it like this here.
                return true;
            }

            if (method is { ReturnsVoid: true, MethodKind: MethodKind.Ordinary, Parameters: { Length: 1 } } &&
                method.Parameters.TrySingle(out var parameter) &&
                parameter.Type.IsEither(KnownSymbol.String, KnownSymbol.PropertyChangedEventArgs, KnownSymbol.LinqExpressionOfT))
            {
                if (method.IsStatic)
                {
                    return PropertyChanged.Find(method.ContainingType) is { IsStatic: true };
                }

                return method.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, compilation);
            }

            return false;
        }
    }
}
