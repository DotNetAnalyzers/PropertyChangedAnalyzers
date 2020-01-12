namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class OnPropertyChanged
    {
        internal static IMethodSymbol? Find(IEventSymbol propertyChanged, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            IMethodSymbol? match = null;
            var containingType = propertyChanged.ContainingType;
            while (propertyChanged != null)
            {
                foreach (var member in propertyChanged.ContainingType.GetMembers())
                {
                    if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } candidate &&
                        candidate.IsStatic == propertyChanged.IsStatic)
                    {
                        if (!Equals(candidate.ContainingType, containingType) &&
                            candidate.DeclaredAccessibility == Accessibility.Private)
                        {
                            continue;
                        }

                        switch (IsMatch(candidate, semanticModel, cancellationToken))
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

                propertyChanged = propertyChanged.OverriddenEvent;
            }

            return match;
        }

        internal static IMethodSymbol? Find(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (PropertyChangedEvent.TryFind(type, out var propertyChangedEvent))
            {
                return Find(propertyChangedEvent, semanticModel, cancellationToken);
            }

            return null;
        }

        internal static OnPropertyChangedMatch? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation is null ||
                invocation.ArgumentList?.Arguments.Count > 1 ||
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

        internal static OnPropertyChangedMatch? Match(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var result = IsMatch(method, semanticModel, cancellationToken);
            if (result != AnalysisResult.No &&
                method.Parameters.TrySingle(out var parameter))
            {
                return new OnPropertyChangedMatch(result, parameter);
            }

            return null;
        }

        internal static AnalysisResult IsMatch(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<IMethodSymbol>? visited = null)
        {
            if (visited?.Add(method) == false)
            {
                return AnalysisResult.No;
            }

            if (!IsPotentialMatch(method, semanticModel.Compilation))
            {
                return AnalysisResult.No;
            }

            // not using known symbol here as both jetbrains & mvvm cross defines a NotifyPropertyChangedInvocatorAttribute
            if (method.GetAttributes().TryFirst(x => x.AttributeClass.Name == "NotifyPropertyChangedInvocatorAttribute", out _))
            {
                return AnalysisResult.Yes;
            }

            var result = AnalysisResult.No;
            if (method.Parameters.TrySingle(out var parameter) &&
                method.TrySingleDeclaration(cancellationToken, out MethodDeclarationSyntax? declaration))
            {
                using var walker = InvocationWalker.Borrow(declaration);
                foreach (var invocation in walker.Invocations)
                {
                    if (invocation is { ArgumentList: { Arguments: { Count: 2 } oneArg } } &&
                        oneArg.TryElementAt(1, out var argument) &&
                        PropertyChangedEvent.IsInvoke(invocation, semanticModel, cancellationToken))
                    {
                        if (argument.Expression is IdentifierNameSyntax identifierName &&
                            identifierName.Identifier.ValueText == parameter.Name)
                        {
                            return AnalysisResult.Yes;
                        }

                        if (PropertyChangedEventArgs.IsCreatedWith(argument.Expression, parameter, semanticModel, cancellationToken))
                        {
                            return AnalysisResult.Yes;
                        }
                    }
                    else if (invocation is { ArgumentList: { Arguments: { Count: 1 } arguments } } &&
                             arguments[0] is { Expression: { } expression } &&
                             invocation.IsPotentialThisOrBase())
                    {
                        if (PropertyChangedEventArgs.IsCreatedWith(expression, parameter, semanticModel, cancellationToken) ||
                            IdentifierNameWalker.Contains(expression, parameter, semanticModel, cancellationToken))
                        {
                            if (semanticModel.TryGetSymbol(invocation, cancellationToken, out var invokedMethod))
                            {
                                using var set = visited.IncrementUsage();
                                switch (IsMatch(invokedMethod, semanticModel, cancellationToken, set))
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
                    return PropertyChangedEvent.TryFind(method.ContainingType, out var @event) &&
                           @event.IsStatic;
                }

                return method.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, compilation);
            }

            return false;
        }
    }
}
