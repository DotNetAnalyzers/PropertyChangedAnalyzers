namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChanged
    {
        internal static AnalysisResult InvokesPropertyChangedFor(ExpressionSyntax mutation, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (mutation.Parent is ArgumentSyntax argument &&
                argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) &&
                argument.Parent is ArgumentListSyntax argumentList &&
                argumentList.Parent is InvocationExpressionSyntax invocation &&
                invocation.IsPotentialThisOrBase())
            {
                if (IsSetAndRaiseCall(invocation, semanticModel, cancellationToken) != AnalysisResult.No &&
                    semanticModel.GetSymbolSafe(invocation, cancellationToken) is IMethodSymbol setAndRaiseMethod &&
                    setAndRaiseMethod.Parameters.TryLast(x => x.Type == KnownSymbol.String, out var nameParameter))
                {
                    if (invocation.TryFindArgument(nameParameter, out var nameArg))
                    {
                        if (nameArg.TryGetStringValue(semanticModel, cancellationToken, out var name))
                        {
                            if (string.IsNullOrEmpty(name) ||
                                name == property.Name)
                            {
                                return AnalysisResult.Yes;
                            }
                        }
                    }
                    else if (invocation.FirstAncestor<PropertyDeclarationSyntax>() is PropertyDeclarationSyntax propertyDeclaration &&
                             propertyDeclaration.Identifier.ValueText == property.Name)
                    {
                        return AnalysisResult.Yes;
                    }
                }
                else if (semanticModel.GetSymbolSafe(invocation, cancellationToken) is IMethodSymbol method &&
                         method.TrySingleDeclaration(cancellationToken, out MethodDeclarationSyntax declaration))
                {
                    switch (Notifies(declaration))
                    {
                        case AnalysisResult.No:
                            break;
                        case AnalysisResult.Yes:
                            return AnalysisResult.Yes;
                        case AnalysisResult.Maybe:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else if (mutation is AssignmentExpressionSyntax assignmentExpression &&
                     (assignmentExpression.Left is IdentifierNameSyntax ||
                      (assignmentExpression.Left is MemberAccessExpressionSyntax memberAccess &&
                       memberAccess.Expression is ThisExpressionSyntax)) &&
                      semanticModel.TryGetSymbol(assignmentExpression.Left, cancellationToken, out IPropertySymbol otherProperty) &&
                      otherProperty.SetMethod.TrySingleDeclaration(cancellationToken, out AccessorDeclarationSyntax otherSetter) &&
                    Notifies(otherSetter) == AnalysisResult.Yes)
            {
                return AnalysisResult.Yes;
            }

            var block = mutation.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Body ??
                        mutation.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body ??
                        mutation.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>()?.Body;

            return Notifies(block);

            AnalysisResult Notifies(SyntaxNode scope)
            {
                if (scope == null)
                {
                    return AnalysisResult.No;
                }

                var result = AnalysisResult.No;
                using (var walker = InvocationWalker.Borrow(scope))
                {
                    foreach (var candidate in walker.Invocations)
                    {
                        if (!candidate.Contains(mutation) &&
                            mutation.IsExecutedBefore(candidate) == ExecutedBefore.No)
                        {
                            continue;
                        }

                        switch (TryGetName(candidate, semanticModel, cancellationToken, out var propertyName))
                        {
                            case AnalysisResult.No:
                                continue;
                            case AnalysisResult.Yes:
                                if (string.IsNullOrEmpty(propertyName) ||
                                    propertyName == property.Name)
                                {
                                    return AnalysisResult.Yes;
                                }

                                continue;
                            case AnalysisResult.Maybe:
                                result = AnalysisResult.Maybe;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                return result;
            }
        }

        internal static AnalysisResult TryGetName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out string propertyName)
        {
            propertyName = null;
            if (IsPropertyChangedInvoke(invocation, semanticModel, cancellationToken))
            {
                if (invocation.ArgumentList.Arguments.TryElementAt(1, out var propertyChangedArg) &&
                    PropertyChangedEventArgs.TryGetPropertyName(propertyChangedArg.Expression, semanticModel, cancellationToken, out propertyName))
                {
                    return AnalysisResult.Yes;
                }

                return AnalysisResult.No;
            }

            if (IsOnPropertyChanged(invocation, semanticModel, cancellationToken, out var onPropertyChanged) != AnalysisResult.No &&
                onPropertyChanged.Parameters.TrySingle(out var parameter))
            {
                if (invocation.ArgumentList.Arguments.Count == 0)
                {
                    if (parameter.IsCallerMemberName())
                    {
                        switch (invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>())
                        {
                            case MethodDeclarationSyntax method:
                                propertyName = method.Identifier.ValueText;
                                return AnalysisResult.Yes;
                            case PropertyDeclarationSyntax property:
                                propertyName = property.Identifier.ValueText;
                                return AnalysisResult.Yes;
                        }
                    }

                    return AnalysisResult.No;
                }

                if (invocation.ArgumentList.Arguments.TrySingle(out var argument))
                {
                    if (parameter.Type == KnownSymbol.String)
                    {
                        return argument.TryGetStringValue(semanticModel, cancellationToken, out propertyName)
                            ? AnalysisResult.Yes
                            : AnalysisResult.No;
                    }

                    if (parameter.Type == KnownSymbol.PropertyChangedEventArgs)
                    {
                        return PropertyChangedEventArgs.TryGetPropertyName(argument.Expression, semanticModel, cancellationToken, out propertyName)
                            ? AnalysisResult.Yes
                            : AnalysisResult.No;
                    }

                    if (argument.Expression is ParenthesizedLambdaExpressionSyntax lambda)
                    {
                        if (semanticModel.GetSymbolSafe(lambda.Body, cancellationToken) is ISymbol property)
                        {
                            propertyName = property.Name;
                            return AnalysisResult.Yes;
                        }

                        return AnalysisResult.No;
                    }
                }
            }

            return AnalysisResult.No;
        }

        internal static bool TryGetOnPropertyChanged(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol invoker)
        {
            if (type.TryFindEventRecursive("PropertyChanged", out var propertyChangedEvent))
            {
                return TryGetOnPropertyChanged(propertyChangedEvent, semanticModel, cancellationToken, out invoker);
            }

            invoker = null;
            return false;
        }

        internal static bool TryGetOnPropertyChanged(IEventSymbol propertyChangedEvent, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol invoker)
        {
            invoker = null;
            var containingType = propertyChangedEvent.ContainingType;
            while (propertyChangedEvent != null)
            {
                foreach (var member in propertyChangedEvent.ContainingType.GetMembers())
                {
                    if (member is IMethodSymbol candidate &&
                        candidate.MethodKind == MethodKind.Ordinary &&
                        candidate.IsStatic == propertyChangedEvent.IsStatic)
                    {
                        if (!Equals(candidate.ContainingType, containingType) &&
                            candidate.DeclaredAccessibility == Accessibility.Private)
                        {
                            continue;
                        }

                        switch (IsOnPropertyChanged(candidate, semanticModel, cancellationToken))
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
                                    (candidate.Parameters.TrySingle(out parameter) &&
                                     parameter.Type == KnownSymbol.String))
                                {
                                    invoker = candidate;
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                propertyChangedEvent = propertyChangedEvent.OverriddenEvent;
            }

            return invoker != null;
        }

        internal static bool IsPropertyChangedInvoke(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation.ArgumentList?.Arguments.Count == 2 &&
                invocation.ArgumentList.Arguments[0].Expression.IsEither(SyntaxKind.ThisExpression, SyntaxKind.NullLiteralExpression) &&
                invocation.IsPotentialReturnVoid())
            {
                switch (invocation.Parent)
                {
                    case ConditionalAccessExpressionSyntax conditionalAccess when IsPotential(conditionalAccess):
                        return semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyChangedEventHandler.Invoke, cancellationToken, out _);
                    case ExpressionStatementSyntax _ when semanticModel.TryGetSymbol(invocation, cancellationToken, out var symbol):
                        return symbol == KnownSymbol.PropertyChangedEventHandler.Invoke;
                    default:
                        return false;
                }
            }

            return false;

            bool IsPotential(ConditionalAccessExpressionSyntax candidate)
            {
                return candidate.Expression is IdentifierNameSyntax ||
                       (candidate.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is ThisExpressionSyntax);
            }
        }

        internal static AnalysisResult IsOnPropertyChanged(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return IsOnPropertyChanged(invocation, semanticModel, cancellationToken, out _);
        }

        internal static AnalysisResult IsOnPropertyChanged(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            method = null;
            if (invocation == null ||
                invocation.ArgumentList?.Arguments.Count > 1 ||
                !invocation.IsPotentialReturnVoid() ||
                !invocation.IsPotentialThisOrBase())
            {
                return AnalysisResult.No;
            }

            if (invocation.FirstAncestor<ClassDeclarationSyntax>() is ClassDeclarationSyntax containingClass)
            {
                if (containingClass.BaseList?.Types == null ||
                    containingClass.BaseList.Types.Count == 0)
                {
                    return AnalysisResult.No;
                }

                if (semanticModel.TryGetSymbol(invocation, cancellationToken, out method))
                {
                    return IsOnPropertyChanged(method, semanticModel, cancellationToken);
                }
            }

            return AnalysisResult.No;
        }

        internal static AnalysisResult IsOnPropertyChanged(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<IMethodSymbol> visited = null)
        {
            if (visited?.Add(method) == false)
            {
                return AnalysisResult.No;
            }

            if (!IsPotentialOnPropertyChanged(method, semanticModel.Compilation))
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
                method.TrySingleDeclaration(cancellationToken, out MethodDeclarationSyntax declaration))
            {
                using (var walker = InvocationWalker.Borrow(declaration))
                {
                    foreach (var invocation in walker.Invocations)
                    {
                        if (invocation.ArgumentList == null ||
                            invocation.ArgumentList.Arguments.Count == 0)
                        {
                            continue;
                        }

                        if (invocation.ArgumentList.Arguments.TryElementAt(1, out var argument) &&
                            IsPropertyChangedInvoke(invocation, semanticModel, cancellationToken))
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
                        else if (invocation.ArgumentList.Arguments.TrySingle(out argument) &&
                                 invocation.IsPotentialThisOrBase())
                        {
                            if (PropertyChangedEventArgs.IsCreatedWith(argument.Expression, parameter, semanticModel, cancellationToken) ||
                                IdentifierNameWalker.Contains(argument.Expression, parameter, semanticModel, cancellationToken))
                            {
                                if (semanticModel.TryGetSymbol(invocation, cancellationToken, out var invokedMethod))
                                {
                                    using (var set = visited.IncrementUsage())
                                    {
                                        switch (IsOnPropertyChanged(invokedMethod, semanticModel, cancellationToken, set))
                                        {
                                            case AnalysisResult.No:
                                                break;
                                            case AnalysisResult.Yes:
                                                return AnalysisResult.Yes;
                                            case AnalysisResult.Maybe:
                                                result = AnalysisResult.Maybe;
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
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

        internal static bool TryGetSetAndRaise(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return type.TryFindFirstMethodRecursive(x => IsSetAndRaise(x, semanticModel, cancellationToken) != AnalysisResult.No, out method);
        }

        internal static AnalysisResult IsSetAndRaiseCall(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<IMethodSymbol> visited = null)
        {
            if (candidate?.ArgumentList == null ||
                candidate.ArgumentList.Arguments.Count < 2 ||
                !candidate.ArgumentList.Arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword), out _) ||
                !candidate.IsPotentialThisOrBase())
            {
                return AnalysisResult.No;
            }

            return IsSetAndRaise(
                semanticModel.GetSymbolSafe(candidate, cancellationToken),
                semanticModel,
                cancellationToken,
                visited);
        }

        internal static AnalysisResult IsSetAndRaise(IMethodSymbol candidate, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<IMethodSymbol> visited = null)
        {
            if (visited?.Add(candidate) == false)
            {
                return AnalysisResult.No;
            }

            if (candidate == null ||
                candidate.IsStatic)
            {
                return AnalysisResult.No;
            }

            if (candidate.MethodKind != MethodKind.Ordinary ||
                candidate.ReturnType != KnownSymbol.Boolean ||
                !candidate.IsGenericMethod ||
                candidate.TypeParameters.Length != 1 ||
                candidate.Parameters.Length < 3 ||
                !candidate.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, semanticModel.Compilation))
            {
                return AnalysisResult.No;
            }

            var type = candidate.TypeArguments.Length == 1
                ? candidate.TypeArguments[0]
                : candidate.TypeParameters[0];
            var parameter = candidate.Parameters[0];
            if (parameter.RefKind != RefKind.Ref ||
                !ReferenceEquals(parameter.Type, type))
            {
                return AnalysisResult.No;
            }

            if (!ReferenceEquals(candidate.Parameters[1].Type, type) ||
                candidate.Parameters[candidate.Parameters.Length - 1].Type != KnownSymbol.String)
            {
                return AnalysisResult.No;
            }

            if (candidate.DeclaringSyntaxReferences.TrySingle(out var reference))
            {
                var syntaxNode = (MethodDeclarationSyntax)reference.GetSyntax();
                using (var walker = InvocationWalker.Borrow(syntaxNode))
                {
                    if (!walker.Invocations.TrySingle(
                        x => IsOnPropertyChanged(x, semanticModel, cancellationToken) != AnalysisResult.No ||
                             IsPropertyChangedInvoke(x, semanticModel, cancellationToken),
                        out _))
                    {
                        using (var set = visited.IncrementUsage())
                        {
                            var result = AnalysisResult.No;
                            foreach (var invocation in walker.Invocations)
                            {
                                switch (IsSetAndRaiseCall(invocation, semanticModel, cancellationToken, set))
                                {
                                    case AnalysisResult.No:
                                        break;
                                    case AnalysisResult.Yes:
                                        return AnalysisResult.Yes;
                                    case AnalysisResult.Maybe:
                                        result = AnalysisResult.Maybe;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            return result;
                        }
                    }
                }

                using (var walker = AssignmentWalker.Borrow(syntaxNode))
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

        private static bool IsPotentialOnPropertyChanged(IMethodSymbol method, Compilation compilation)
        {
            if (method == KnownSymbol.MvvmCrossMvxNotifyPropertyChanged.RaisePropertyChanged ||
                method == KnownSymbol.MvvmCrossMvxNotifyPropertyChanged.RaisePropertyChangedOfT ||
                method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChanged ||
                method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChangedOfT)
            {
                // They changed return type to task, special casing it like this here.
                return true;
            }

            if (method != null &&
                method.ReturnsVoid &&
                method.MethodKind == MethodKind.Ordinary &&
                method.Parameters.TrySingle(out var parameter) &&
                parameter.Type.IsEither(KnownSymbol.String, KnownSymbol.PropertyChangedEventArgs, KnownSymbol.LinqExpressionOfT))
            {
                if (method.IsStatic)
                {
                    return method.ContainingType.TryFindEvent("PropertyChanged", out var @event) &&
                           @event.IsStatic;
                }

                return method.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, compilation);
            }

            return false;
        }
    }
}
