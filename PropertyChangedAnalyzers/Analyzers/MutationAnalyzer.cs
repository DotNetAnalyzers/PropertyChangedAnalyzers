namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class MutationAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly string PropertyNameKey = "PropertyName";

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC003NotifyForDependentProperty);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(c => HandlePrefixUnaryExpression(c), SyntaxKind.PreIncrementExpression);
            context.RegisterSyntaxNodeAction(c => HandlePrefixUnaryExpression(c), SyntaxKind.PreDecrementExpression);

            context.RegisterSyntaxNodeAction(c => HandlePostfixUnaryExpression(c), SyntaxKind.PostIncrementExpression);
            context.RegisterSyntaxNodeAction(c => HandlePostfixUnaryExpression(c), SyntaxKind.PostDecrementExpression);

            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.AndAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.OrAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.ExclusiveOrAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.AddAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.DivideAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.LeftShiftAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.ModuloAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.MultiplyAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.RightShiftAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.SubtractAssignmentExpression);
            context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeAction(c => HandleArgument(c), SyntaxKind.Argument);
        }

        private static void HandlePostfixUnaryExpression(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                !IsInIgnoredScope(context) &&
                context.Node is PostfixUnaryExpressionSyntax postfix &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, postfix.Operand, out var backing))
            {
                Handle(postfix, backing, context);
            }
        }

        private static void HandlePrefixUnaryExpression(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                !IsInIgnoredScope(context) &&
                context.Node is PrefixUnaryExpressionSyntax prefix &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, prefix.Operand, out var backing))
            {
                Handle(prefix, backing, context);
            }
        }

        private static void HandleAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                !IsInIgnoredScope(context) &&
                context.Node is AssignmentExpressionSyntax assignment &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, assignment.Left, out var backing))
            {
                Handle(assignment, backing, context);
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                !IsInIgnoredScope(context) &&
                context.Node is ArgumentSyntax argument &&
                argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, argument.Expression, out var backing))
            {
                Handle(argument.Expression, backing, context);
            }
        }

        private static void Handle(ExpressionSyntax mutation, ExpressionSyntax backing, SyntaxNodeAnalysisContext context)
        {
            if (context.ContainingSymbol.ContainingType is INamedTypeSymbol containingType &&
                containingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation) &&
                mutation.TryFirstAncestorOrSelf<TypeDeclarationSyntax>(out var typeDeclaration))
            {
                using (var pathWalker = MemberPath.PathWalker.Borrow(backing))
                {
                    if (pathWalker.Tokens.Count == 0)
                    {
                        return;
                    }

                    foreach (var member in typeDeclaration.Members)
                    {
                        if (member is PropertyDeclarationSyntax propertyDeclaration &&
                            propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword) &&
                            TryGeExpressionBodyOrGetter(propertyDeclaration, out var getter) &&
                            !getter.Contains(backing) &&
                            PropertyPath.Uses(getter, pathWalker, context) &&
                            !Property.IsLazy(propertyDeclaration, context.SemanticModel, context.CancellationToken) &&
                            context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken) is IPropertySymbol property &&
                            PropertyChanged.InvokesPropertyChangedFor(mutation, property, context.SemanticModel, context.CancellationToken) == AnalysisResult.No)
                        {
                            if (context.Node.TryFirstAncestor(out PropertyDeclarationSyntax? inProperty) &&
                                ReferenceEquals(inProperty, propertyDeclaration) &&
                                Property.TrySingleReturned(inProperty, out var returned) &&
                                PropertyPath.Uses(backing, returned, context))
                            {
                                // We let INPC002 handle this
                                continue;
                            }

                            var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(PropertyNameKey, property.Name), });
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC003NotifyForDependentProperty, context.Node.GetLocation(), properties, property.Name));
                        }
                    }
                }
            }
        }

        private static bool IsInIgnoredScope(SyntaxNodeAnalysisContext context)
        {
            if (context.ContainingSymbol is IMethodSymbol method &&
                method.Name == "Dispose")
            {
                return true;
            }

            if (context.Node.FirstAncestorOrSelf<InitializerExpressionSyntax>() != null ||
                context.Node.FirstAncestorOrSelf<ConstructorDeclarationSyntax>() != null)
            {
                if (context.Node.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>() != null)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private static bool TryGetAssignedExpression(ITypeSymbol containingType, SyntaxNode node, out ExpressionSyntax backing)
        {
            backing = null;
            if (node.IsMissing)
            {
                return false;
            }

            if (node is IdentifierNameSyntax identifierName &&
                !IdentifierTypeWalker.IsLocalOrParameter(identifierName) &&
                !containingType.TryFindProperty(identifierName.Identifier.ValueText, out _))
            {
                backing = identifierName;
            }
            else if (node is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression is ThisExpressionSyntax &&
                    containingType.TryFindProperty(memberAccess.Name.Identifier.ValueText, out _))
                {
                    return false;
                }

                backing = memberAccess;
            }

            return backing != null;
        }

        private static bool TryGeExpressionBodyOrGetter(PropertyDeclarationSyntax property, out SyntaxNode node)
        {
            if (property.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
            {
                node = expressionBody.Expression;
                return true;
            }

            node = null;
            if (property.TryGetGetter(out var getter))
            {
                node = (SyntaxNode)getter.Body ?? getter.ExpressionBody;
            }

            return node != null;
        }
    }
}
