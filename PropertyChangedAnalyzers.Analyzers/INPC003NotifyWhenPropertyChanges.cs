namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
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
                TryGetAssignedExpression(expression.Operand, out var backing))
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
                TryGetAssignedExpression(expression.Operand, out var backing))
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
                TryGetAssignedExpression(expression.Left, out var backing))
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
                TryGetAssignedExpression(argument.Expression, out var backing))
            {
                Handle(context, backing);
            }
        }

        private static bool TryGetAssignedExpression(SyntaxNode node, out ExpressionSyntax backing)
        {
            backing = null;
            if (node.IsMissing)
            {
                return false;
            }

            if (node is IdentifierNameSyntax identifierName &&
                !IdentifierTypeWalker.IsLocalOrParameter(identifierName))
            {
                backing = identifierName;
            }
            else if (node is MemberAccessExpressionSyntax memberAccess)
            {
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

            if (context.Node.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is PropertyDeclarationSyntax inProperty &&
                Property.IsSimplePropertyWithBackingField(inProperty, context.SemanticModel, context.CancellationToken))
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
                    var propertyDeclaration = member as PropertyDeclarationSyntax;
                    if (propertyDeclaration == null ||
                        !propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword) ||
                        Property.IsLazy(propertyDeclaration, context.SemanticModel, context.CancellationToken))
                    {
                        continue;
                    }

                    var getter = ExpressionBodyOrGetter(propertyDeclaration);
                    if (getter == null)
                    {
                        continue;
                    }

                    var accessor = context.Node.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
                    if (accessor?.IsKind(SyntaxKind.GetAccessorDeclaration) == true &&
                        accessor.FirstAncestorOrSelf<PropertyDeclarationSyntax>() == propertyDeclaration)
                    {
                        continue;
                    }

                    var expressionBody = context.Node.FirstAncestorOrSelf<ArrowExpressionClauseSyntax>();
                    if (expressionBody?.FirstAncestorOrSelf<PropertyDeclarationSyntax>() == propertyDeclaration)
                    {
                        continue;
                    }

                    using (var walker = TouchedFieldsWalker.Borrow(getter, context.SemanticModel, context.CancellationToken))
                    {
                        if (walker.Contains(backing))
                        {
                            if (context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken) is IPropertySymbol property &&
                                PropertyChanged.InvokesPropertyChangedFor(context.Node, property, context.SemanticModel, context.CancellationToken) == AnalysisResult.No)
                            {
                                var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(PropertyNameKey, property.Name), });
                                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties, property.Name));
                            }
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

        private static SyntaxNode ExpressionBodyOrGetter(PropertyDeclarationSyntax property)
        {
            if (property.ExpressionBody != null)
            {
                return property.ExpressionBody.Expression;
            }

            if (property.TryGetGetter(out var getter))
            {
                return (SyntaxNode)getter.Body ?? getter.ExpressionBody;
            }

            return null;
        }

        private sealed class TouchedFieldsWalker : PooledWalker<TouchedFieldsWalker>
        {
            private readonly List<ExpressionSyntax> backings = new List<ExpressionSyntax>();
            private readonly HashSet<SyntaxNode> visited = new HashSet<SyntaxNode>();

            private SemanticModel semanticModel;
            private CancellationToken cancellationToken;

            private TouchedFieldsWalker()
            {
            }

            public static TouchedFieldsWalker Borrow(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                var pooled = Borrow(() => new TouchedFieldsWalker());
                pooled.semanticModel = semanticModel;
                pooled.cancellationToken = cancellationToken;
                pooled.Visit(node);
                return pooled;
            }

            public bool Contains(ExpressionSyntax expression)
            {
                foreach (var backing in this.backings)
                {
                    if (MemberPath.Equals(backing, expression))
                    {
                        return true;
                    }
                }

                return false;
            }

            public override void Visit(SyntaxNode node)
            {
                if (this.visited.Add(node))
                {
                    base.Visit(node);
                }
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (!IdentifierTypeWalker.IsLocalOrParameter(node))
                {
                    this.backings.Add(node);
                    var symbol = this.semanticModel.GetSymbolSafe(node, this.cancellationToken);
                    if (symbol is IPropertySymbol property &&
                        property.TrySingleDeclaration(this.cancellationToken, out var propertyDeclaration) &&
                        ExpressionBodyOrGetter(propertyDeclaration) is SyntaxNode propertyBody)
                    {
                        this.Visit(propertyBody);
                    }
                    else if (symbol is IMethodSymbol method &&
                             method.TrySingleDeclaration(this.cancellationToken, out var methodDeclaration))
                    {
                        if (methodDeclaration.Body is BlockSyntax body)
                        {
                            this.Visit(body);
                        }
                        else if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
                        {
                            this.Visit(expressionBody);
                        }
                    }
                }

                base.VisitIdentifierName(node);
            }

            protected override void Clear()
            {
                this.backings.Clear();
                this.visited.Clear();
                this.semanticModel = null;
                this.cancellationToken = CancellationToken.None;
            }
        }
    }
}
