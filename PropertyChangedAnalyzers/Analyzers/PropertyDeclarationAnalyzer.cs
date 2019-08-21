namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropertyDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC002MutablePublicPropertyShouldNotify,
            Descriptors.INPC010GetAndSetSame,
            Descriptors.INPC015PropertyIsRecursive,
            Descriptors.INPC017BackingFieldNameMisMatch,
            Descriptors.INPC019GetBackingField,
            Descriptors.INPC020PreferExpressionBodyAccessor,
            Descriptors.INPC021SetBackingField);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                context.ContainingSymbol is IPropertySymbol property)
            {
                using (var walker = ReturnExpressions(propertyDeclaration))
                {
                    foreach (var returnValue in walker.ReturnValues)
                    {
                        if (IsProperty(returnValue, property))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC015PropertyIsRecursive, returnValue.GetLocation(), "Getter returns property, infinite recursion"));
                        }
                    }

                    if (walker.ReturnValues.TrySingle(out var single))
                    {
                        if (single.IsEither(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.IdentifierName) &&
                            MemberPath.TrySingle(single, out var path) &&
                            context.SemanticModel.TryGetSymbol(path, context.CancellationToken, out IFieldSymbol backingField))
                        {
                            if (!HasMatchingName(backingField, property))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC017BackingFieldNameMisMatch, path.GetLocation()));
                            }

                            if (propertyDeclaration.TryGetSetter(out var setAccessor))
                            {
                                using (var mutationWalker = MutationWalker.Borrow(setAccessor, SearchScope.Member, context.SemanticModel, context.CancellationToken))
                                {
                                    if (mutationWalker.IsEmpty)
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC021SetBackingField, setAccessor.GetLocation()));
                                    }
                                }
                            }
                        }

                        if (single is LiteralExpressionSyntax &&
                            propertyDeclaration.TryGetSetter(out var set) &&
                            Property.TryGetSingleAssignedWithParameter(set, context.SemanticModel, context.CancellationToken, out var fieldAccess))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC019GetBackingField,
                                    single.GetLocation(),
                                    additionalLocations: new[] { fieldAccess.GetLocation() }));
                        }
                    }
                }

                if (propertyDeclaration.TryGetSetter(out var setter))
                {
                    if (ShouldBeExpressionBody(setter))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC020PreferExpressionBodyAccessor, setter.GetLocation()));
                    }

                    using (var assignmentWalker = AssignmentWalker.Borrow(setter))
                    {
                        if (assignmentWalker.Assignments.TryFirst(x => IsProperty(x.Left, property), out var recursiveAssignment))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC015PropertyIsRecursive, recursiveAssignment.Left.GetLocation(), "Setter assigns property, infinite recursion"));
                        }

                        if (propertyDeclaration.TryGetGetter(out var getter))
                        {
                            if (property.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation) &&
                                Property.ShouldNotify(propertyDeclaration, property, context.SemanticModel, context.CancellationToken))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC002MutablePublicPropertyShouldNotify, propertyDeclaration.GetLocation(), property.Name));
                            }

                            if (GetAndSetsSameField(assignmentWalker, getter, context) == false)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC010GetAndSetSame, propertyDeclaration.GetLocation()));
                            }

                            if (ShouldBeExpressionBody(getter))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC020PreferExpressionBodyAccessor, getter.GetLocation()));
                            }
                        }
                    }
                }
            }
        }

        private static ReturnExpressionsWalker ReturnExpressions(PropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration.TryGetGetter(out var getter))
            {
                return ReturnExpressionsWalker.Borrow(getter);
            }

            if (propertyDeclaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
            {
                return ReturnExpressionsWalker.Borrow(expressionBody);
            }

            return ReturnExpressionsWalker.Empty();
        }

        private static bool HasMatchingName(IFieldSymbol backingField, IPropertySymbol property)
        {
            if (backingField.IsStatic || backingField.IsConst)
            {
                return true;
            }

            if (property.ExplicitInterfaceImplementations.TryFirst(out var explicitProperty))
            {
                return HasMatchingName(backingField, explicitProperty);
            }

            if (backingField.Name.Length < property.Name.Length)
            {
                return false;
            }

            var diff = backingField.Name.Length - property.Name.Length;
            for (var pi = property.Name.Length - 1; pi >= 0; pi--)
            {
                var fi = pi + diff;
                if (pi == 0)
                {
                    if (char.ToLower(property.Name[pi]) != backingField.Name[fi])
                    {
                        return false;
                    }

                    switch (fi)
                    {
                        case 0:
                            return true;
                        case 1:
                            return backingField.Name[0] == '_' ||
                                   backingField.Name[0] == '@';
                        default:
                            return false;
                    }
                }

                if (property.Name[pi] != backingField.Name[fi])
                {
                    if (pi == 0 ||
                        !char.IsUpper(property.Name[pi - 1]) ||
                        char.ToUpper(backingField.Name[fi]) != property.Name[pi])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsProperty(ExpressionSyntax expression, IPropertySymbol property)
        {
            if (expression is MemberAccessExpressionSyntax memberAccess &&
                !(memberAccess.Expression is ThisExpressionSyntax))
            {
                return false;
            }

            if (property.ExplicitInterfaceImplementations.Any())
            {
                return false;
            }

            if (TryGetMemberName(expression, out var name))
            {
                return name == property.Name;
            }

            return false;
        }

        private static bool TryGetMemberName(ExpressionSyntax expression, out string name)
        {
            if (expression is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
                return true;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InstanceExpressionSyntax &&
                memberAccess.Name is IdentifierNameSyntax nameIdentifier)
            {
                name = nameIdentifier.Identifier.ValueText;
                return true;
            }

            name = null;
            return false;
        }

        private static bool ShouldBeExpressionBody(AccessorDeclarationSyntax accessor)
        {
            return accessor.Body is BlockSyntax block &&
                   block.Statements.TrySingle(out var statement) &&
                   statement.IsEither(SyntaxKind.ReturnStatement, SyntaxKind.ExpressionStatement);
        }

        private static bool? GetAndSetsSameField(AssignmentWalker assignmentWalker, AccessorDeclarationSyntax getter, SyntaxNodeAnalysisContext context, PooledSet<ISymbol> visited = null)
        {
            if (assignmentWalker.Assignments.TrySingle(out var singleAssignment) &&
                ReturnExpressionsWalker.TryGetSingle(getter, out var singleReturnValue))
            {
                if (PropertyPath.Uses(singleAssignment.Left, singleReturnValue, context))
                {
                    return true;
                }

                if (context.SemanticModel.TryGetSymbol(singleAssignment.Left, context.CancellationToken, out var setSymbol) &&
                    context.SemanticModel.TryGetSymbol(singleReturnValue, context.CancellationToken, out var getSymbol))
                {
                    if (getSymbol.Kind == setSymbol.Kind)
                    {
                        return false;
                    }

                    if (getSymbol.Kind == SymbolKind.Property)
                    {
                        using (visited = visited.IncrementUsage())
                        {
                            if (visited.Add(getSymbol) &&
                                getSymbol.TrySingleDeclaration(context.CancellationToken, out AccessorDeclarationSyntax otherGetter))
                            {
                                return GetAndSetsSameField(assignmentWalker, otherGetter, context);
                            }
                        }

                        return false;
                    }
                }
            }

            return null;
        }
    }
}
