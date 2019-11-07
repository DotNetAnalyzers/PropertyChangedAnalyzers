namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
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
                context.Node is PostfixUnaryExpressionSyntax { Operand: { } operand } postfix &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, operand, out var backing))
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
                context.Node is AssignmentExpressionSyntax { Left: { } left } assignment &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, left, out var backing))
            {
                Handle(assignment, backing, context);
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                !IsInIgnoredScope(context) &&
                context.Node is ArgumentSyntax { Expression: { } expression } argument &&
                argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, expression, out var backing))
            {
                Handle(argument.Expression, backing, context);
            }
        }

        private static void Handle(ExpressionSyntax mutation, ExpressionSyntax backing, SyntaxNodeAnalysisContext context)
        {
            if (context.ContainingSymbol.ContainingType is { } containingType &&
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
                            context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken) is { } property &&
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
            return context switch
            {
                { ContainingSymbol: IMethodSymbol { Name: "Dispose" } } => true,
                { Node: { Parent: ObjectCreationExpressionSyntax _ } } => true,
                { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } } => !context.Node.TryFirstAncestor<AnonymousFunctionExpressionSyntax>(out _),
                _ => false
            };
        }

        private static bool TryGetAssignedExpression(ITypeSymbol containingType, SyntaxNode node, [NotNullWhen(true)] out ExpressionSyntax? backing)
        {
            switch (node)
            {
                case IdentifierNameSyntax identifierName
                    when !IdentifierTypeWalker.IsLocalOrParameter(identifierName) &&
                         !containingType.TryFindProperty(identifierName.Identifier.ValueText, out _):
                    backing = identifierName;
                    return true;
                case MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: { } name } memberAccess
                    when !containingType.TryFindProperty(name.Identifier.ValueText, out _):
                    backing = memberAccess;
                    return true;
                case MemberAccessExpressionSyntax memberAccess:
                    backing = memberAccess;
                    return true;
                default:
                    backing = null;
                    return false;
            }
        }

        private static bool TryGeExpressionBodyOrGetter(PropertyDeclarationSyntax property, [NotNullWhen(true)] out SyntaxNode? node)
        {
            switch (property)
            {
                case { ExpressionBody: { Expression: { } expression } }:
                    node = expression;
                    return true;
                case { AccessorList: { } }
                    when property.TryGetGetter(out var getter):
                    node = (SyntaxNode)getter.Body ?? getter.ExpressionBody;
                    return node != null;
                default:
                    node = null;
                    return false;
            }
        }
    }
}
