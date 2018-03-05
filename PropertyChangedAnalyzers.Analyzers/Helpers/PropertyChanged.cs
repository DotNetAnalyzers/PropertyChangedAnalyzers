namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChanged
    {
        internal static AnalysisResult InvokesPropertyChangedFor(SyntaxNode assignment, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var invokes = AnalysisResult.No;
            var argument = assignment.FirstAncestorOrSelf<ArgumentSyntax>();
            if (argument != null)
            {
                var setMethod = (IMethodSymbol)semanticModel.GetSymbolSafe(
                    argument.FirstAncestorOrSelf<InvocationExpressionSyntax>(),
                    cancellationToken);
                if (IsSetAndRaiseMethod(setMethod, semanticModel, cancellationToken) &&
                    setMethod.Parameters[setMethod.Parameters.Length - 1].IsCallerMemberName())
                {
                    var inProperty = semanticModel.GetDeclaredSymbolSafe(argument.FirstAncestorOrSelf<PropertyDeclarationSyntax>(), cancellationToken);
                    if (inProperty?.Name == property.Name)
                    {
                        return AnalysisResult.Yes;
                    }
                }
            }

            var block = assignment.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Body ??
                        assignment.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body ??
                        assignment.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>()?.Body;
            if (block == null)
            {
                return AnalysisResult.No;
            }

            using (var walker = InvocationWalker.Borrow(block))
            {
                foreach (var invocation in walker.Invocations)
                {
                    if (invocation.SpanStart < assignment.SpanStart)
                    {
                        continue;
                    }

                    switch (TryGetInvokedPropertyChangedName(invocation, semanticModel, cancellationToken, out ArgumentSyntax _, out var propertyName))
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
                            invokes = AnalysisResult.Maybe;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return invokes;
        }

        internal static AnalysisResult TryGetInvokedPropertyChangedName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string propertyName)
        {
            nameArg = null;
            propertyName = null;
            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            if (method == null)
            {
                return AnalysisResult.No;
            }

            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke)
            {
                if (invocation.ArgumentList.Arguments.TryElementAt(1, out var propertyChangedArg))
                {
                    if (TryGetCreatePropertyChangedEventArgsFor(propertyChangedArg.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }

                    if (TryGetCachedArgs(propertyChangedArg, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }
                }

                return AnalysisResult.Maybe;
            }

            if (IsPropertyChangedInvoker(method, semanticModel, cancellationToken) == AnalysisResult.No)
            {
                return AnalysisResult.No;
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                if (method.Parameters[0].IsCallerMemberName())
                {
                    var member = invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                    if (member == null)
                    {
                        return AnalysisResult.Maybe;
                    }

                    propertyName = semanticModel.GetDeclaredSymbolSafe(member, cancellationToken)?.Name;
                    if (propertyName != null)
                    {
                        return AnalysisResult.Yes;
                    }

                    return AnalysisResult.Maybe;
                }
            }

            if (invocation.ArgumentList.Arguments.TrySingle(out var argument))
            {
                if (TryGetCreatePropertyChangedEventArgsFor(argument.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                {
                    return AnalysisResult.Yes;
                }

                if (argument.Expression is ParenthesizedLambdaExpressionSyntax lambda)
                {
                    if (semanticModel.GetSymbolSafe(lambda.Body, cancellationToken) is ISymbol property)
                    {
                        propertyName = property.Name;
                        nameArg = argument;
                        return AnalysisResult.Yes;
                    }

                    return AnalysisResult.No;
                }

                var symbol = semanticModel.GetTypeInfoSafe(argument.Expression, cancellationToken).Type;
                if (symbol == KnownSymbol.String)
                {
                    if (argument.TryGetStringValue(semanticModel, cancellationToken, out propertyName))
                    {
                        nameArg = argument;
                        return AnalysisResult.Yes;
                    }

                    return AnalysisResult.Maybe;
                }

                if (symbol == KnownSymbol.PropertyChangedEventArgs)
                {
                    if (TryGetCreatePropertyChangedEventArgsFor(argument.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }

                    if (TryGetCachedArgs(argument, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }
                }
            }

            return AnalysisResult.Maybe;
        }

        internal static bool TryGetInvoker(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol invoker)
        {
            if (type.TryGetEvent("PropertyChanged", out var @event))
            {
                return TryGetInvoker(@event, semanticModel, cancellationToken, out invoker);
            }

            invoker = null;
            return false;
        }

        internal static bool TryGetInvoker(IEventSymbol @event, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol invoker)
        {
            if (@event == null)
            {
                invoker = null;
                return false;
            }

            invoker = null;
            if (@event.IsStatic)
            {
                foreach (var member in @event.ContainingType.GetMembers())
                {
                    if (member is IMethodSymbol candidate &&
                        candidate.IsStatic)
                    {
                        switch (IsPropertyChangedInvoker(candidate, semanticModel, cancellationToken))
                        {
                            case AnalysisResult.No:
                                continue;
                            case AnalysisResult.Yes:
                                invoker = candidate;
                                if (invoker.Parameters.Length == 1 &&
                                    invoker.Parameters[0]
                                           .Type == KnownSymbol.String)
                                {
                                    return true;
                                }

                                break;
                            case AnalysisResult.Maybe:
                                invoker = candidate;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                return invoker != null;
            }

            var type = @event.ContainingType;
            while (type != null)
            {
                foreach (var member in type.GetMembers())
                {
                    if (member is IMethodSymbol candidate &&
                        !candidate.IsStatic &&
                        candidate.MethodKind == MethodKind.Ordinary)
                    {
                        if (candidate.DeclaredAccessibility == Accessibility.Private &&
                            candidate.ContainingType != @event.ContainingType)
                        {
                            continue;
                        }

                        switch (IsPropertyChangedInvoker(candidate, semanticModel, cancellationToken))
                        {
                            case AnalysisResult.No:
                                continue;
                            case AnalysisResult.Yes:
                                invoker = candidate;
                                if (invoker.Parameters.Length == 1 &&
                                    invoker.Parameters[0]
                                           .Type == KnownSymbol.String)
                                {
                                    return true;
                                }

                                break;
                            case AnalysisResult.Maybe:
                                invoker = candidate;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                type = type.BaseType;
            }

            return invoker != null;
        }

        internal static bool IsPotentialInvoker(IMethodSymbol method)
        {
            if (method == null ||
                method.IsStatic ||
                !method.ReturnsVoid ||
                method.Parameters.Length != 1 ||
                method.MethodKind != MethodKind.Ordinary ||
                method.AssociatedSymbol != null ||
                !method.ContainingType.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return false;
            }

            return true;
        }

        internal static bool IsPropertyChangedInvoker(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation == null ||
                invocation.ArgumentList?.Arguments.Count > 1 ||
                invocation.Parent is ArgumentSyntax ||
                invocation.Parent is EqualsValueClauseSyntax ||
                invocation.Parent is AssignmentExpressionSyntax)
            {
                return false;
            }

            if (invocation.Parent is IfStatementSyntax ifStatement &&
                ifStatement.Condition.Contains(invocation))
            {
                return false;
            }

            if (invocation.Expression is IdentifierNameSyntax ||
               (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is IdentifierNameSyntax &&
                memberAccess.Expression is ThisExpressionSyntax))
            {
                if (invocation.FirstAncestor<ClassDeclarationSyntax>() is ClassDeclarationSyntax containingClass)
                {
                    if (containingClass.BaseList?.Types == null ||
                        containingClass.BaseList.Types.Count == 0)
                    {
                        return false;
                    }

                    return IsPropertyChangedInvoker(semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol, semanticModel, cancellationToken) == AnalysisResult.Yes;
                }

                return false;
            }

            return false;
        }

        internal static AnalysisResult IsPropertyChangedInvoker(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, PooledHashSet<IMethodSymbol> @checked = null)
        {
            if (@checked?.Add(method) == false)
            {
                return AnalysisResult.No;
            }

            if (!IsPotentialInvoker(method))
            {
                return AnalysisResult.No;
            }

            var parameter = method.Parameters[0];
            if (method.DeclaringSyntaxReferences.Length == 0)
            {
                if (method == KnownSymbol.MvvmLightViewModelBase.RaisePropertyChanged ||
                    method == KnownSymbol.MvvmLightViewModelBase.RaisePropertyChangedOfT ||
                    method == KnownSymbol.MvvmLightObservableObject.RaisePropertyChanged ||
                    method == KnownSymbol.MvvmLightObservableObject.RaisePropertyChangedOfT ||
                    method == KnownSymbol.CaliburnMicroPropertyChangedBase.NotifyOfPropertyChange ||
                    method == KnownSymbol.CaliburnMicroPropertyChangedBase.NotifyOfPropertyChangeOfT ||
                    method == KnownSymbol.StyletPropertyChangedBase.NotifyOfPropertyChange ||
                    method == KnownSymbol.StyletPropertyChangedBase.NotifyOfPropertyChangeOfT ||
                    method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChanged ||
                    method == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.RaisePropertyChangedOfT ||
                    method == KnownSymbol.MicrosoftPracticesPrismMvvmBindableBase.OnPropertyChanged ||
                    method == KnownSymbol.MicrosoftPracticesPrismMvvmBindableBase.OnPropertyChangedOfT)
                {
                    return AnalysisResult.Yes;
                }

                if (parameter.Type == KnownSymbol.String ||
                    parameter.Type == KnownSymbol.PropertyChangedEventArgs)
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

            if (parameter.Type != KnownSymbol.String &&
                parameter.Type != KnownSymbol.PropertyChangedEventArgs &&
                parameter.Type != KnownSymbol.LinqExpressionOfT)
            {
                return AnalysisResult.No;
            }

            foreach (var declaration in method.Declarations(cancellationToken))
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

                        var invokedMethod = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                        if (invokedMethod == null)
                        {
                            continue;
                        }

                        if ((invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                             !(memberAccess.Expression is ThisExpressionSyntax)) ||
                            (invocation.Expression is MemberBindingExpressionSyntax))
                        {
                            if (invokedMethod == KnownSymbol.PropertyChangedEventHandler.Invoke)
                            {
                                if (invocation.ArgumentList.Arguments.TryElementAt(1, out var argument))
                                {
                                    var identifier = argument.Expression as IdentifierNameSyntax;
                                    if (identifier?.Identifier.ValueText == parameter.Name)
                                    {
                                        return AnalysisResult.Yes;
                                    }

                                    if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                                    {
                                        var nameArgument = objectCreation.ArgumentList.Arguments[0];
                                        if ((nameArgument.Expression as IdentifierNameSyntax)?.Identifier.ValueText ==
                                            parameter.Name)
                                        {
                                            return AnalysisResult.Yes;
                                        }
                                    }
                                }
                            }

                            return AnalysisResult.No;
                        }

                        if (invokedMethod == KnownSymbol.PropertyChangedEventHandler.Invoke)
                        {
                            if (invocation.ArgumentList.Arguments.TryElementAt(1, out var argument))
                            {
                                var identifier = argument.Expression as IdentifierNameSyntax;
                                if (identifier?.Identifier.ValueText == parameter.Name)
                                {
                                    return AnalysisResult.Yes;
                                }

                                if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                                {
                                    var nameArgument = objectCreation.ArgumentList.Arguments[0];
                                    if ((nameArgument.Expression as IdentifierNameSyntax)?.Identifier.ValueText ==
                                        parameter.Name)
                                    {
                                        return AnalysisResult.Yes;
                                    }
                                }
                            }

                            return AnalysisResult.No;
                        }

                        if (!method.ContainingType.Is(invokedMethod.ContainingType))
                        {
                            continue;
                        }

                        using (var argsWalker = IdentifierNameWalker.Borrow(invocation.ArgumentList))
                        {
                            if (argsWalker.Contains(parameter, semanticModel, cancellationToken))
                            {
                                using (var set = PooledHashSet<IMethodSymbol>.Borrow(@checked))
                                {
                                    return IsPropertyChangedInvoker(invokedMethod, semanticModel, cancellationToken, set);
                                }
                            }
                        }
                    }
                }
            }

            return AnalysisResult.No;
        }

        internal static bool IsCallerMemberName(this IMethodSymbol invoker)
        {
            return invoker != null &&
                   invoker.Parameters.Length == 1 &&
                   invoker.Parameters[0].Type == KnownSymbol.String &&
                   invoker.Parameters[0].IsCallerMemberName();
        }

        internal static bool IsCallerMemberName(this IParameterSymbol parameter)
        {
            if (parameter.HasExplicitDefaultValue)
            {
                foreach (var attribute in parameter.GetAttributes())
                {
                    if (attribute.AttributeClass == KnownSymbol.CallerMemberNameAttribute)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsNotifyPropertyChanged(StatementSyntax statement, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var expressionStatement = statement as ExpressionStatementSyntax;
            var expression = expressionStatement?.Expression;
            if (expression == null)
            {
                return false;
            }

            if (expression is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                return IsNotifyPropertyChanged(conditionalAccess.WhenNotNull as InvocationExpressionSyntax, semanticModel, cancellationToken);
            }

            return IsNotifyPropertyChanged(expression as InvocationExpressionSyntax, semanticModel, cancellationToken);
        }

        internal static bool IsNotifyPropertyChanged(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation == null)
            {
                return false;
            }

            if (invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count <= 1)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    !(memberAccess.Expression is InstanceExpressionSyntax))
                {
                    return false;
                }

                var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                return IsPropertyChangedInvoker(method, semanticModel, cancellationToken) != AnalysisResult.No;
            }

            if (invocation.ArgumentList?.Arguments.Count == 2 &&
                invocation.ArgumentList.Arguments[0].Expression is ThisExpressionSyntax)
            {
                var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                return method == KnownSymbol.PropertyChangedEventHandler.Invoke;
            }

            return false;
        }

        internal static bool TryGetSetAndRaiseMethod(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            if (type.TryFirstMethod(x => IsSetAndRaiseMethod(x, semanticModel, cancellationToken), out method))
            {
                return true;
            }

            if (type.Is(KnownSymbol.MvvmLightViewModelBase))
            {
                return type.TryFirstMember(
                    "Set",
                    x => IsSetAndRaiseMethod(x, semanticModel, cancellationToken),
                    out method);
            }

            if (type.Is(KnownSymbol.CaliburnMicroPropertyChangedBase))
            {
                return type.TryFirstMember(
                    "Set",
                    x => IsSetAndRaiseMethod(x, semanticModel, cancellationToken),
                    out method);
            }

            return false;
        }

        internal static bool IsSetAndRaiseCall(InvocationExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, PooledHashSet<IMethodSymbol> @checked = null)
        {
            if (candidate?.ArgumentList == null ||
                candidate.ArgumentList.Arguments.Count < 2 ||
                candidate.ArgumentList.Arguments.Count > 3)
            {
                return false;
            }

            if (!candidate.ArgumentList.Arguments[0]
                          .RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword))
            {
                return false;
            }

            return IsSetAndRaiseMethod(
                semanticModel.GetSymbolSafe(candidate, cancellationToken) as IMethodSymbol,
                semanticModel,
                cancellationToken,
                @checked);
        }

        internal static bool IsSetAndRaiseMethod(IMethodSymbol candidate, SemanticModel semanticModel, CancellationToken cancellationToken, PooledHashSet<IMethodSymbol> @checked = null)
        {
            if (@checked?.Add(candidate) == false)
            {
                return false;
            }

            if (candidate == null ||
                candidate.IsStatic)
            {
                return false;
            }

            if (candidate.MethodKind != MethodKind.Ordinary ||
                candidate.ReturnType != KnownSymbol.Boolean ||
                !candidate.IsGenericMethod ||
                candidate.TypeParameters.Length != 1 ||
                candidate.Parameters.Length < 3 ||
                    !candidate.ContainingType.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return false;
            }

            var type = candidate.TypeArguments.Length == 1
                ? candidate.TypeArguments[0]
                : candidate.TypeParameters[0];
            var parameter = candidate.Parameters[0];
            if (parameter.RefKind != RefKind.Ref ||
                !ReferenceEquals(parameter.Type, type))
            {
                return false;
            }

            if (!ReferenceEquals(candidate.Parameters[1].Type, type) ||
                candidate.Parameters[candidate.Parameters.Length - 1].Type != KnownSymbol.String)
            {
                return false;
            }

            if (candidate.DeclaringSyntaxReferences.TrySingle(out var reference))
            {
                var syntaxNode = (MethodDeclarationSyntax)reference.GetSyntax();
                using (var walker = InvocationWalker.Borrow(syntaxNode))
                {
                    if (!walker.Invocations.TrySingle(
                        x => IsNotifyPropertyChanged(x, semanticModel, cancellationToken),
                        out _))
                    {
                        using (var set = PooledHashSet<IMethodSymbol>.Borrow(@checked))
                        {
                            // ReSharper disable once AccessToDisposedClosure R# does not figure things out here.
                            return walker.Invocations.TrySingle(
                                x => IsSetAndRaiseCall(x, semanticModel, cancellationToken, set),
                                out _);
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
                        return false;
                    }
                }

                return true;
            }

            return candidate == KnownSymbol.MvvmLightViewModelBase.Set ||
                   candidate == KnownSymbol.MvvmLightObservableObject.Set ||
                   candidate == KnownSymbol.CaliburnMicroPropertyChangedBase.Set ||
                   candidate == KnownSymbol.StyletPropertyChangedBase.SetAndNotify ||
                   candidate == KnownSymbol.MvvmCrossCoreMvxNotifyPropertyChanged.SetProperty ||
                   candidate == KnownSymbol.MicrosoftPracticesPrismMvvmBindableBase.SetProperty;
        }

        private static bool TryGetCachedArgs(
            ArgumentSyntax propertyChangedArg,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out ArgumentSyntax nameArg,
            out string propertyName)
        {
            var cached = semanticModel.GetSymbolSafe(propertyChangedArg.Expression, cancellationToken);
            if (cached is IFieldSymbol)
            {
                foreach (var syntaxReference in cached.DeclaringSyntaxReferences)
                {
                    var declarator = syntaxReference.GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
                    if (TryGetCreatePropertyChangedEventArgsFor(
                        declarator?.Initializer?.Value as ObjectCreationExpressionSyntax,
                        semanticModel,
                        cancellationToken,
                        out nameArg,
                        out propertyName))
                    {
                        {
                            return true;
                        }
                    }
                }
            }

            if (cached is IPropertySymbol)
            {
                foreach (var syntaxReference in cached.DeclaringSyntaxReferences)
                {
                    var propertyDeclaration = syntaxReference.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
                    if (TryGetCreatePropertyChangedEventArgsFor(
                        propertyDeclaration?.Initializer?.Value as ObjectCreationExpressionSyntax,
                        semanticModel,
                        cancellationToken,
                        out nameArg,
                        out propertyName))
                    {
                        {
                            return true;
                        }
                    }
                }
            }

            nameArg = null;
            propertyName = null;
            return false;
        }

        private static bool TryGetCreatePropertyChangedEventArgsFor(this ExpressionSyntax newPropertyChangedEventArgs, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string propertyName)
        {
            nameArg = null;
            propertyName = null;
            var objectCreation = newPropertyChangedEventArgs as ObjectCreationExpressionSyntax;
            if (objectCreation == null)
            {
                return false;
            }

            if (objectCreation.ArgumentList?.Arguments.TrySingle(out nameArg) == true)
            {
                return nameArg.TryGetStringValue(semanticModel, cancellationToken, out propertyName);
            }

            return false;
        }
    }
}
