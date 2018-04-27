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
    internal class INPC003NotifyWhenPropertyChanges : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC003";
        public static readonly string PropertyNameKey = "PropertyName";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Notify when property changes.",
            messageFormat: "Notify that property '{0}' changes.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Notify when property changes.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(HandlePrefixUnaryExpression, SyntaxKind.PreIncrementExpression);
            context.RegisterSyntaxNodeAction(HandlePrefixUnaryExpression, SyntaxKind.PreDecrementExpression);

            context.RegisterSyntaxNodeAction(HandlePostfixUnaryExpression, SyntaxKind.PostIncrementExpression);
            context.RegisterSyntaxNodeAction(HandlePostfixUnaryExpression, SyntaxKind.PostDecrementExpression);

            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.AndAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.OrAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.ExclusiveOrAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.AddAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.DivideAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.LeftShiftAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.ModuloAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.MultiplyAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.RightShiftAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.SubtractAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeAction(HandleArgument, SyntaxKind.Argument);
        }

        private static void HandlePostfixUnaryExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis() ||
                IsInIgnoredScope(context))
            {
                return;
            }

            if (context.Node is PostfixUnaryExpressionSyntax expression &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, expression.Operand, out var backing))
            {
                Handle(context, backing);
            }
        }

        private static void HandlePrefixUnaryExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis() ||
                IsInIgnoredScope(context))
            {
                return;
            }

            if (context.Node is PrefixUnaryExpressionSyntax expression &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, expression.Operand, out var backing))
            {
                Handle(context, backing);
            }
        }

        private static void HandleAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis() ||
                IsInIgnoredScope(context))
            {
                return;
            }

            if (context.Node is AssignmentExpressionSyntax expression &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, expression.Left, out var backing))
            {
                Handle(context, backing);
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis() ||
                IsInIgnoredScope(context))
            {
                return;
            }

            if (context.Node is ArgumentSyntax argument &&
                argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) &&
                TryGetAssignedExpression(context.ContainingSymbol.ContainingType, argument.Expression, out var backing))
            {
                Handle(context, backing);
            }
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

        private static void Handle(SyntaxNodeAnalysisContext context, ExpressionSyntax backing)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (IsInIgnoredScope(context))
            {
                return;
            }

            var typeSymbol = context.ContainingSymbol.ContainingType;
            if (!typeSymbol.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return;
            }

            var typeDeclaration = context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            using (var pathWalker = MemberPath.PathWalker.Borrow(backing))
            {
                if (pathWalker.IdentifierNames.Count == 0)
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
                        PropertyChanged.InvokesPropertyChangedFor(context.Node, property, context.SemanticModel, context.CancellationToken) == AnalysisResult.No)
                    {
                        if (context.Node.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is PropertyDeclarationSyntax inProperty &&
                            ReferenceEquals(inProperty, propertyDeclaration) &&
                            Property.TrySingleReturnedInGetter(inProperty, out var returned) &&
                            PropertyPath.Uses(backing, returned, context))
                        {
                            // We let INPC002 handle this
                            continue;
                        }

                        var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(PropertyNameKey, property.Name), });
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties, property.Name));
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

            if (!context.ContainingSymbol.ContainingType.Is(KnownSymbol.INotifyPropertyChanged))
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

        private static bool TryGeExpressionBodyOrGetter(PropertyDeclarationSyntax property, out SyntaxNode node)
        {
            node = null;
            if (property.ExpressionBody != null)
            {
                node = property.ExpressionBody.Expression;
            }
            else if (property.TryGetGetter(out var getter))
            {
                node = (SyntaxNode)getter.Body ?? getter.ExpressionBody;
            }

            return node != null;
        }
    }
}
