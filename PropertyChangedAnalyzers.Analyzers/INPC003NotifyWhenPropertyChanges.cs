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
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var expression = (PostfixUnaryExpressionSyntax)context.Node;
            if (TryGetAssignedField(expression.Operand, context.SemanticModel, context.CancellationToken, out var field))
            {
                Handle(context, field);
            }
        }

        private static void HandlePrefixUnaryExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var expression = (PrefixUnaryExpressionSyntax)context.Node;
            if (TryGetAssignedField(expression.Operand, context.SemanticModel, context.CancellationToken, out var field))
            {
                Handle(context, field);
            }
        }

        private static void HandleAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var expression = (AssignmentExpressionSyntax)context.Node;
            if (TryGetAssignedField(expression.Left, context.SemanticModel, context.CancellationToken, out var field))
            {
                Handle(context, field);
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var argument = (ArgumentSyntax)context.Node;
            if (argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword))
            {
                if (TryGetAssignedField(argument.Expression, context.SemanticModel, context.CancellationToken, out var field))
                {
                    Handle(context, field);
                }
            }
        }

        private static bool TryGetAssignedField(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (node.IsMissing)
            {
                return false;
            }

            if (node is IdentifierNameSyntax identifierName)
            {
                field = semanticModel.GetSymbolSafe(identifierName, cancellationToken) as IFieldSymbol;
                return field != null;
            }

            if (node is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression is ThisExpressionSyntax &&
                    memberAccess.Name is IdentifierNameSyntax)
                {
                    field = semanticModel.GetSymbolSafe(memberAccess.Name, cancellationToken) as IFieldSymbol;
                }

                return field != null;
            }

            return false;
        }

        private static void Handle(SyntaxNodeAnalysisContext context, IFieldSymbol assignedField)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (IsInIgnoredScope(context))
            {
                return;
            }

            var typeDeclaration = context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            var typeSymbol = context.SemanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);
            if (!typeSymbol.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return;
            }

            if (!typeSymbol.Equals(assignedField.ContainingType))
            {
                return;
            }

            var inProperty = context.Node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            if (inProperty != null)
            {
                if (Property.IsSimplePropertyWithBackingField(inProperty, context.SemanticModel, context.CancellationToken))
                {
                    return;
                }
            }

            using (var set = PooledHashSet<IPropertySymbol>.Borrow())
            {
                foreach (var member in typeDeclaration.Members)
                {
                    var propertyDeclaration = member as PropertyDeclarationSyntax;
                    if (propertyDeclaration == null ||
                        propertyDeclaration.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                        propertyDeclaration.Modifiers.Any(SyntaxKind.ProtectedKeyword) ||
                        Property.IsLazy(propertyDeclaration, context.SemanticModel, context.CancellationToken))
                    {
                        continue;
                    }

                    var getter = GetterBody(propertyDeclaration);
                    if (getter == null)
                    {
                        continue;
                    }

                    var property = context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);
                    if (property == null ||
                        property.DeclaredAccessibility == Accessibility.Private ||
                        property.DeclaredAccessibility == Accessibility.Protected)
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
                        if (walker.Contains(assignedField))
                        {
                            if (PropertyChanged.InvokesPropertyChangedFor(context.Node, property, context.SemanticModel, context.CancellationToken) == AnalysisResult.No)
                            {
                                if (set.Add(property))
                                {
                                    var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(PropertyNameKey, property.Name), });
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), properties, property.Name));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsInIgnoredScope(SyntaxNodeAnalysisContext context)
        {
            var method = context.ContainingSymbol as IMethodSymbol;
            if (method?.Name == "Dispose")
            {
                return true;
            }

            if (context.Node.FirstAncestorOrSelf<InitializerExpressionSyntax>() != null)
            {
                return true;
            }

            if (context.Node.FirstAncestorOrSelf<ConstructorDeclarationSyntax>() != null)
            {
                if (context.Node.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>() != null)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private static SyntaxNode GetterBody(PropertyDeclarationSyntax property)
        {
            if (property.ExpressionBody != null)
            {
                return property.ExpressionBody.Expression;
            }

            if (property.TryGetGetAccessorDeclaration(out var getter))
            {
                return (SyntaxNode)getter.Body ?? getter.ExpressionBody;
            }

            return null;
        }

        private sealed class TouchedFieldsWalker : PooledWalker<TouchedFieldsWalker>
        {
            private readonly HashSet<IFieldSymbol> fields = new HashSet<IFieldSymbol>();
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

            public bool Contains(IFieldSymbol field) => this.fields.Contains(field);

            public override void Visit(SyntaxNode node)
            {
                if (this.visited.Add(node))
                {
                    base.Visit(node);
                }
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                var symbol = this.semanticModel.GetSymbolSafe(node, this.cancellationToken);
                if (symbol is IFieldSymbol field)
                {
                    this.fields.Add(field);
                }

                if (symbol is IPropertySymbol property)
                {
                    if (property.GetMethod != null)
                    {
                        foreach (var getter in property.GetMethod.Declarations(this.cancellationToken))
                        {
                            this.Visit(getter);
                        }
                    }
                }

                if (symbol is IMethodSymbol method)
                {
                    foreach (var declaration in method.Declarations(this.cancellationToken))
                    {
                        this.Visit(declaration);
                    }
                }

                base.VisitIdentifierName(node);
            }

            protected override void Clear()
            {
                this.fields.Clear();
                this.visited.Clear();
                this.semanticModel = null;
                this.cancellationToken = CancellationToken.None;
            }
        }
    }
}