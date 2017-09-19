namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyCodeFixProvider))]
    [Shared]
    internal class MakePropertyNotifyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC002MutablePublicPropertyShouldNotify.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => BatchFixer.Default;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                 .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var propertyDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
                var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                if (typeDeclaration == null)
                {
                    continue;
                }

                var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);

                if (Property.IsMutableAutoProperty(propertyDeclaration) ||
                    IsSimpleAssignmentOnly(propertyDeclaration, out _, out _))
                {
                    if (PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out IMethodSymbol invoker) &&
                        invoker.Parameters[0].Type == KnownSymbol.String)
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Convert to notifying property.",
                                cancellationToken => MakeNotifyAsync(context, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                this.GetType().FullName),
                            diagnostic);
                    }
                }
            }
        }

        private static async Task<Document> MakeNotifyAsync(CodeFixContext context, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
                                             .ConfigureAwait(false);
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return context.Document;
            }

            var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
            var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
            var fieldAccess = "missing";
            if (Property.IsMutableAutoProperty(propertyDeclaration))
            {
                var declaredSymbol = (INamedTypeSymbol)editor.SemanticModel.GetDeclaredSymbolSafe(classDeclaration, cancellationToken);
                var name = usesUnderscoreNames
                    ? $"_{property.Name.ToFirstCharLower()}"
                    : property.Name.ToFirstCharLower();
                while (declaredSymbol.MemberNames.Contains(name))
                {
                    name += "_";
                }

                fieldAccess = usesUnderscoreNames
                    ? name
                    : "this." + name;

                var backingField = (FieldDeclarationSyntax) editor.Generator.FieldDeclaration(
                    name,
                    accessibility: Accessibility.Private,
                    modifiers: DeclarationModifiers.None,
                    type: propertyDeclaration.Type,
                    initializer: propertyDeclaration.Initializer?.Value);
                var index = classDeclaration.Members.IndexOf(propertyDeclaration);
                for (int i = index; i < classDeclaration.Members.Count; i++)
                {
                    if(classDeclaration.Members[i] is PropertyDeclarationSyntax 
                }

                editor.AddField(classDeclaration, backingField);
            }

            if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out var field))
            {
                fieldAccess = field.ToFullString();
            }

            return context.Document;
        }

        private static Fix CreateFix(
            SyntaxGenerator syntaxGenerator,
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property,
            IMethodSymbol invoker,
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            string backingFieldName;
            if (Property.IsMutableAutoProperty(propertyDeclaration))
            {
                backingFieldName = MakePropertyNotifyHelper.BackingFieldNameForAutoProperty(propertyDeclaration, usesUnderscoreNames: true);
                var backingField = (FieldDeclarationSyntax)syntaxGenerator.FieldDeclaration(
                    backingFieldName,
                    propertyDeclaration.Type,
                    Accessibility.Private,
                    DeclarationModifiers.None,
                    propertyDeclaration.Initializer?.Value);

                var notifyingProperty = propertyDeclaration.WithoutInitializer()
                                                           .WithGetterReturningBackingField(
                                                               syntaxGenerator,
                                                               backingFieldName)
                                                           .WithNotifyingSetter(
                                                               property,
                                                               syntaxGenerator,
                                                               backingFieldName,
                                                               invoker,
                                                               diagnosticOptions);
                return new Fix(propertyDeclaration, notifyingProperty, backingField);
            }

            if (IsSimpleAssignmentOnly(
                propertyDeclaration,
                out ExpressionStatementSyntax assignStatement,
                out var fieldAccess))
            {
                var notifyingProperty = propertyDeclaration.WithGetterReturningBackingField(
                                                               syntaxGenerator,
                                                               fieldAccess)
                                                           .WithNotifyingSetter(
                                                               property,
                                                               syntaxGenerator,
                                                               assignStatement,
                                                               fieldAccess,
                                                               invoker,
                                                               diagnosticOptions);
                return new Fix(propertyDeclaration, notifyingProperty, null);
            }

            return new Fix(propertyDeclaration, null, null);
        }

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax propertyDeclaration, out ExpressionStatementSyntax assignStatement, out ExpressionSyntax fieldAccess)
        {
            fieldAccess = null;
            assignStatement = null;
            if (!propertyDeclaration.TryGetSetAccessorDeclaration(out AccessorDeclarationSyntax setter) ||
                setter.Body == null ||
                setter.Body.Statements.Count != 1)
            {
                return false;
            }

            if (Property.AssignsValueToBackingField(setter, out var assignment))
            {
                assignStatement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                fieldAccess = assignment.Left;
                return assignStatement != null;
            }

            return false;
        }

        private struct Fix
        {
            internal readonly PropertyDeclarationSyntax OldProperty;
            internal readonly PropertyDeclarationSyntax NotifyingProperty;
            internal readonly FieldDeclarationSyntax BackingField;

            public Fix(PropertyDeclarationSyntax oldProperty, PropertyDeclarationSyntax notifyingProperty, FieldDeclarationSyntax backingField)
            {
                this.OldProperty = oldProperty;
                this.NotifyingProperty = notifyingProperty;
                this.BackingField = backingField;
            }
        }

        private class BatchFixer : FixAllProvider
        {
            public static readonly BatchFixer Default = new BatchFixer();
            private static readonly ImmutableArray<FixAllScope> SupportedFixAllScopes = ImmutableArray.Create(FixAllScope.Document);

            private BatchFixer()
            {
            }

            public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
            {
                return SupportedFixAllScopes;
            }

            [SuppressMessage("ReSharper", "RedundantCaseLabel", Justification = "Mute R#")]
            public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        return this.FixDocumentAsync(fixAllContext);
                    case FixAllScope.Project:
                    case FixAllScope.Solution:
                    case FixAllScope.Custom:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private async Task<CodeAction> FixDocumentAsync(FixAllContext context)
            {
                var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                 .ConfigureAwait(false);
                var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);

                var diagnostics = await context.GetDocumentDiagnosticsAsync(context.Document).ConfigureAwait(false);
                var fixes = new List<Fix>();
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Id != INPC002MutablePublicPropertyShouldNotify.DiagnosticId)
                    {
                        continue;
                    }

                    var propertyDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();

                    var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                    if (typeDeclaration == null)
                    {
                        continue;
                    }

                    var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);
                    var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);

                    if (PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out IMethodSymbol invoker) &&
                        invoker.Parameters[0].Type == KnownSymbol.String)
                    {
                        var fix = CreateFix(
                            syntaxGenerator,
                            propertyDeclaration,
                            property,
                            invoker,
                            context.Document.Project.CompilationOptions.SpecificDiagnosticOptions);

                        if (fix.NotifyingProperty == propertyDeclaration)
                        {
                            continue;
                        }

                        fixes.Add(fix);
                    }
                }

                var fixedTypes = new List<FixedTypes>();
                foreach (var typeFixes in fixes.GroupBy(x => x.OldProperty.FirstAncestorOrSelf<TypeDeclarationSyntax>()))
                {
                    var fixedTypeDeclaration = typeFixes.Key.ReplaceNodes(
                        typeFixes.Select(x => x.OldProperty),
                        (o, r) => typeFixes.Single(x => x.OldProperty == o)
                                           .NotifyingProperty);

                    foreach (var fix in typeFixes)
                    {
                        fixedTypeDeclaration = fixedTypeDeclaration.WithBackingField(
                            syntaxGenerator,
                            fix.BackingField,
                            fix.NotifyingProperty);
                    }

                    fixedTypes.Add(new FixedTypes(typeFixes.Key, fixedTypeDeclaration));
                }

                return CodeAction.Create(
                    "Convert to notifying property.",
                    _ =>
                        Task.FromResult(
                            context.Document.WithSyntaxRoot(
                                syntaxRoot.ReplaceNodes(fixedTypes.Select(x => x.OldType), (o, r) => fixedTypes.Single(x => x.OldType == o).FixedType))),
                    this.GetType().FullName);
            }
        }
    }
}
