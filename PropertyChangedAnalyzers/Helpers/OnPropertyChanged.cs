namespace PropertyChangedAnalyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class OnPropertyChanged
    {
        internal static bool TryFind(IEventSymbol propertyChangedEvent, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? invoker)
        {
            invoker = null;
            var containingType = propertyChangedEvent.ContainingType;
            while (propertyChangedEvent != null)
            {
                foreach (var member in propertyChangedEvent.ContainingType.GetMembers())
                {
                    if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } candidate &&
                        candidate.IsStatic == propertyChangedEvent.IsStatic)
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
                                invoker = candidate;
                                if (candidate.Parameters.TrySingle(out var parameter) &&
                                    parameter.Type == KnownSymbol.String)
                                {
                                    return true;
                                }

                                break;
                            case AnalysisResult.Maybe:
                                if (invoker is null ||
                                    (candidate.Parameters.TrySingle<IParameterSymbol>(out parameter) &&
                                     parameter.Type == KnownSymbol.String))
                                {
                                    invoker = candidate;
                                }

                                break;
                            default:
                                throw new InvalidOperationException("Unknown AnalysisResult");
                        }
                    }
                }

                propertyChangedEvent = propertyChangedEvent.OverriddenEvent;
            }

            return invoker != null;
        }

        internal static bool TryFind(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? invoker)
        {
            if (PropertyChangedEvent.TryFind(type, out var propertyChangedEvent))
            {
                return TryFind(propertyChangedEvent, semanticModel, cancellationToken, out invoker);
            }

            invoker = null;
            return false;
        }

        internal static AnalysisResult IsMatch(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return IsMatch(invocation, semanticModel, cancellationToken, out _);
        }

        internal static AnalysisResult IsMatch(ExpressionStatementSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (candidate.Expression is InvocationExpressionSyntax invocation)
            {
                return IsMatch(invocation, semanticModel, cancellationToken, out _);
            }

            return AnalysisResult.No;
        }

        internal static AnalysisResult IsMatch(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol? method)
        {
            method = null;
            if (invocation == null ||
                invocation.ArgumentList?.Arguments.Count > 1 ||
                !invocation.IsPotentialReturnVoid() ||
                !invocation.IsPotentialThisOrBase())
            {
                return AnalysisResult.No;
            }

            if (invocation.TryFirstAncestor(out ClassDeclarationSyntax? containingClass))
            {
                if (containingClass.BaseList?.Types == null ||
                    containingClass.BaseList.Types.Count == 0)
                {
                    return AnalysisResult.No;
                }

                if (semanticModel.TryGetSymbol(invocation, cancellationToken, out method))
                {
                    return IsMatch(method, semanticModel, cancellationToken);
                }
            }

            return AnalysisResult.No;
        }

        internal static AnalysisResult IsMatch(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out IParameterSymbol parameter)
        {
            var result = IsMatch(method, semanticModel, cancellationToken);
            if (result == AnalysisResult.No)
            {
                parameter = null;
                return AnalysisResult.No;
            }

            if (method.Parameters.TrySingle(out parameter))
            {
                return result;
            }

            parameter = null;
            return AnalysisResult.No;
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
                using (var walker = InvocationWalker.Borrow(declaration))
                {
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
                                    using (var set = visited.IncrementUsage())
                                    {
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
