namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyFix))]
    [Shared]
    internal class MakePropertyNotifyFix : DocumentEditorCodeFixProvider
    {
        private const string NotifyWhenValueChanges = "Notify when value changes.";

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC002MutablePublicPropertyShouldNotify.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                      .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out PropertyDeclarationSyntax? propertyDeclaration) &&
                    propertyDeclaration.Parent is ClassDeclarationSyntax classDeclarationSyntax &&
                    semanticModel.TryGetSymbol(classDeclarationSyntax, context.CancellationToken, out var type))
                {
                    if (TrySet.TryFind(type, semanticModel, context.CancellationToken, out var trySetMethod) &&
                        TrySet.CanCreateInvocation(trySetMethod, out _))
                    {
                        if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter))
                        {
                            context.RegisterCodeFix(
                                trySetMethod.DisplaySignature(),
                                (editor, cancellationToken) => TrySet(editor, cancellationToken),
                                trySetMethod.MetadataName,
                                diagnostic);

                            async Task TrySet(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                var fieldAccess = await editor.AddBackingFieldAsync(propertyDeclaration, cancellationToken)
                                                              .ConfigureAwait(false);
                                var trySet = await editor.TrySetInvocationAsync(trySetMethod, fieldAccess, InpcFactory.Value, propertyDeclaration, cancellationToken)
                                                         .ConfigureAwait(false);

                                _ = editor.ReplaceNode(
                                              getter,
                                              x => x.AsExpressionBody(fieldAccess))
                                          .ReplaceNode(
                                              setter,
                                              x => x.AsExpressionBody(trySet));

                                if (propertyDeclaration.Initializer != null)
                                {
                                    editor.ReplaceNode(
                                        propertyDeclaration,
                                        (node, g) => ((PropertyDeclarationSyntax)node).WithoutInitializer());
                                }

                                _ = editor.FormatNode(propertyDeclaration);
                            }
                        }
                        else if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out var assignment))
                        {
                            context.RegisterCodeFix(
                                trySetMethod.DisplaySignature(),
                                async (editor, cancellationToken) =>
                                {
                                    var trySet = await editor.TrySetInvocationAsync(trySetMethod, assignment.Left, assignment.Right, propertyDeclaration, cancellationToken)
                                                             .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                        assignment,
                                        x => trySet);
                                },
                                trySetMethod.MetadataName,
                                diagnostic);
                        }
                    }

                    if (OnPropertyChanged.TryFind(type, semanticModel, context.CancellationToken, out var invoker) &&
                        invoker.Parameters.TrySingle(out var parameter) &&
                        parameter.Type.IsEither(KnownSymbol.String, KnownSymbol.PropertyChangedEventArgs))
                    {
                        if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter) &&
                            semanticModel.TryGetSymbol(propertyDeclaration, context.CancellationToken, out var property))
                        {
                            context.RegisterCodeFix(
                                NotifyWhenValueChanges,
                                (editor, cancellationToken) =>
                                    NotifyWhenValueChangesAsync(editor, cancellationToken),
                                nameof(NotifyWhenValueChangesAsync),
                                diagnostic);

                            async Task NotifyWhenValueChangesAsync(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                var backingField = await editor.AddBackingFieldAsync(propertyDeclaration, cancellationToken)
                                                               .ConfigureAwait(false);
                                var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                    .ConfigureAwait(false);
                                _ = editor.ReplaceNode(
                                    getter,
                                    x => x.AsExpressionBody(backingField)
                                          .WithTrailingLineFeed());
                                _ = editor.ReplaceNode(
                                    setter,
                                    x => x.AsBlockBody(
                                        InpcFactory.IfReturn(
                                            InpcFactory.Equals(property.Type, InpcFactory.Value, backingField, editor.SemanticModel)),
                                        SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, backingField, InpcFactory.Value)),
                                        onPropertyChanged));
                                if (propertyDeclaration.Initializer != null)
                                {
                                    _ = editor.ReplaceNode(
                                        propertyDeclaration,
                                        x => x.WithoutInitializer());
                                }

                                _ = editor.FormatNode(propertyDeclaration);
                            }
                        }
                        else if (IsSimpleAssignmentOnly(propertyDeclaration, out setter, out var assignment))
                        {
                            context.RegisterCodeFix(
                                NotifyWhenValueChanges,
                                (editor, cancellationToken) => NotifyWhenValueChangesAsync(editor, cancellationToken),
                                nameof(NotifyWhenValueChangesAsync),
                                diagnostic);

                            context.RegisterCodeFix(
                                "Notify.",
                                (editor, cancellationToken) => NotifyAsync(editor, cancellationToken),
                                nameof(NotifyAsync),
                                diagnostic);

                            async Task NotifyWhenValueChangesAsync(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                if (setter.ExpressionBody != null)
                                {
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                        .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                        setter,
                                        x => x.AsBlockBody(
                                            InpcFactory.IfReturn(
                                                InpcFactory.Equals(assignment.Right, assignment.Left, editor.SemanticModel, cancellationToken)),
                                            SyntaxFactory.ExpressionStatement(assignment),
                                            onPropertyChanged));
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                                else if (setter.Body is { Statements: { Count: 1 } } body &&
                                         body.Statements.TrySingle(out var statement))
                                {
                                    editor.InsertBefore(
                                        statement,
                                        InpcFactory.IfReturn(
                                            InpcFactory.Equals(assignment.Right, assignment.Left, editor.SemanticModel, cancellationToken)));
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                         .ConfigureAwait(false);
                                    editor.InsertAfter(statement, onPropertyChanged);
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                            }

                            async Task NotifyAsync(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                if (setter.ExpressionBody != null)
                                {
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                        .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                        setter,
                                        x => x.AsBlockBody(
                                            SyntaxFactory.ExpressionStatement(assignment),
                                            onPropertyChanged));
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                                else if (setter.Body is { Statements: { Count: 1 } } body &&
                                         body.Statements.TrySingle(out var statement))
                                {
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                        .ConfigureAwait(false);
                                    editor.InsertAfter(statement, onPropertyChanged);
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax property, [NotNullWhen(true)] out AccessorDeclarationSyntax? setter, [NotNullWhen(true)] out AssignmentExpressionSyntax? assignment)
        {
            assignment = null;
            return property.TryGetSetter(out setter) &&
                   Setter.AssignsValueToBackingField(setter, out assignment) &&
                   IsSimple(setter);

            static bool IsSimple(AccessorDeclarationSyntax localSetter)
            {
                if (localSetter.ExpressionBody != null)
                {
                    return true;
                }

                return localSetter.Body is { Statements: { Count: 1 } };
            }
        }
    }
}
