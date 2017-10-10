namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using PropertyChangedAnalyzers.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyPropertyChangedCodeFixProvider))]
    [Shared]
    internal class NotifyPropertyChangedCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC003NotifyWhenPropertyChanges.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentOnlyFixAllProvider.Default;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            var usesUnderscoreNames = syntaxRoot.UsesUnderscoreNames(semanticModel, context.CancellationToken);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(INPC003NotifyWhenPropertyChanges.PropertyNameKey, out var property))
                {
                    var expression = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                               .FirstAncestorOrSelf<ExpressionSyntax>();
                    var typeDeclaration = expression.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                    var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);
                    if (PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out var invoker) &&
                        invoker.Parameters[0].Type == KnownSymbol.String)
                    {
                        var invocation = expression.FirstAncestorOrSelf<InvocationExpressionSyntax>();
                        var method = (IMethodSymbol)semanticModel.GetSymbolSafe(invocation, context.CancellationToken);
                        if (PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, context.CancellationToken))
                        {
                            if (invocation.Parent is ExpressionStatementSyntax)
                            {
                                context.RegisterCodeFix(
                                    CodeAction.Create(
                                        "Notify property change.",
                                        cancellationToken => MakeNotifyCreateIfAsync(
                                            context.Document,
                                            invocation,
                                            property,
                                            invoker,
                                            usesUnderscoreNames,
                                            cancellationToken),
                                        this.GetType()
                                            .Name),
                                    diagnostic);
                                continue;
                            }

                            if (invocation.Parent is IfStatementSyntax ifStatement)
                            {
                                context.RegisterCodeFix(
                                    CodeAction.Create(
                                        "Notify property change.",
                                        cancellationToken => MakeNotifyInIfAsync(
                                            context.Document,
                                            ifStatement,
                                            property,
                                            invoker,
                                            usesUnderscoreNames,
                                            cancellationToken),
                                        this.GetType()
                                            .Name),
                                    diagnostic);
                                continue;
                            }
                        }

                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Notify property change.",
                                cancellationToken => MakeNotifyAsync(
                                    context.Document,
                                    expression,
                                    property,
                                    invoker,
                                    usesUnderscoreNames,
                                    cancellationToken),
                                this.GetType().Name),
                            diagnostic);
                    }
                }
            }
        }

        private static async Task<Document> MakeNotifyAsync(Document document, ExpressionSyntax assignment, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var onPropertyChanged = SyntaxFactory.ParseStatement(OnPropertyChanged(invoker, propertyName, usesUnderscoreNames))
                                                 .WithSimplifiedNames()
                                                 .WithLeadingElasticLineFeed()
                                                 .WithTrailingElasticLineFeed()
                                                 .WithAdditionalAnnotations(Formatter.Annotation);
            var assignStatement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            if (assignment.Parent is AnonymousFunctionExpressionSyntax anonymousFunction)
            {
                if (anonymousFunction.Body is BlockSyntax block)
                {
                    var previousStatement = InsertAfter(block, assignStatement, invoker);
                    editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
                    return editor.GetChangedDocument();
                }

                var expressionStatement = (ExpressionStatementSyntax)editor.Generator.ExpressionStatement(anonymousFunction.Body);
                var withStatements = editor.Generator.WithStatements(anonymousFunction, new[] { expressionStatement, onPropertyChanged });
                editor.ReplaceNode(anonymousFunction, withStatements);
                return editor.GetChangedDocument();
            }
            else if (assignStatement?.Parent is BlockSyntax block)
            {
                var previousStatement = InsertAfter(block, assignStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
                return editor.GetChangedDocument();
            }

            return document;
        }

        private static async Task<Document> MakeNotifyCreateIfAsync(Document document, InvocationExpressionSyntax invocation, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var assignStatement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            if (assignStatement?.Parent is BlockSyntax)
            {
                editor.ReplaceNode(
                    assignStatement,
                    (node, _) =>
                    {
                        using (var pooled = StringBuilderPool.Borrow())
                        {
                            var code = pooled.Item.AppendLine($"if ({invocation.ToFullString()})")
                                             .AppendLine("{")
                                             .AppendLine($"    {OnPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                             .AppendLine("}")
                                             .ToString();

                            return SyntaxFactory.ParseStatement(code)
                                                .WithSimplifiedNames()
                                                .WithLeadingElasticLineFeed()
                                                .WithTrailingElasticLineFeed()
                                                .WithAdditionalAnnotations(Formatter.Annotation);
                        }
                    });
                return editor.GetChangedDocument();
            }

            return document;
        }

        private static async Task<Document> MakeNotifyInIfAsync(Document document, IfStatementSyntax ifStatement, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var onPropertyChanged = SyntaxFactory.ParseStatement(OnPropertyChanged(invoker, propertyName, usesUnderscoreNames))
                                                 .WithSimplifiedNames()
                                                 .WithLeadingElasticLineFeed()
                                                 .WithTrailingElasticLineFeed()
                                                 .WithAdditionalAnnotations(Formatter.Annotation);

            if (ifStatement.Statement is BlockSyntax block)
            {
                if (block.Statements.Count == 0)
                {
                    editor.ReplaceNode(
                        block,
                        block.AddStatements(onPropertyChanged));
                    return editor.GetChangedDocument();
                }

                var previousStatement = InsertAfter(block, ifStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
                return editor.GetChangedDocument();
            }

            editor.ReplaceNode(
                ifStatement.Statement,
                (node, _) =>
                {
                    using (var pooled = StringBuilderPool.Borrow())
                    {
                        var code = pooled.Item.AppendLine("{")
                                              .AppendLine($"{ifStatement.Statement.ToFullString().TrimEnd('\r', '\n')}")
                                              .AppendLine($"    {OnPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                              .AppendLine("}")
                                              .ToString();

                        return SyntaxFactory.ParseStatement(code)
                                            .WithSimplifiedNames()
                                            .WithTrailingElasticLineFeed()
                                            .WithAdditionalAnnotations(Formatter.Annotation);
                    }
                });

            return editor.GetChangedDocument();
        }

        private static string OnPropertyChanged(IMethodSymbol invoker, string propertyName, bool usesUnderscoreNames)
        {
            if (invoker == null)
            {
                return usesUnderscoreNames
                    ? $"PropertyChanged?.Invoke(new System.ComponentModel.PropertyChangedEventArgs({propertyName}));"
                    : $"this.PropertyChanged?.Invoke(new System.ComponentModel.PropertyChangedEventArgs(nameof(this.{propertyName})));";
            }

            if (invoker.Parameters.TryGetSingle(out var parameter))
            {
                if (parameter.Type == KnownSymbol.String)
                {
                    return usesUnderscoreNames
                        ? $"{invoker.Name}(nameof({propertyName}));"
                        : $"this.{invoker.Name}(nameof(this.{propertyName}));";
                }

                if (parameter.Type == KnownSymbol.PropertyChangedEventArgs)
                {
                    return usesUnderscoreNames
                        ? $"{invoker.Name}(new System.ComponentModel.PropertyChangedEventArgs({propertyName}));"
                        : $"this.{invoker.Name}(new System.ComponentModel.PropertyChangedEventArgs(nameof(this.{propertyName})));";
                }
            }

            return "GeneratedSyntaxErrorBugInPropertyChangedAnalyzersCodeFixes";
        }

        private static StatementSyntax InsertAfter(BlockSyntax block, StatementSyntax assignStatement, IMethodSymbol invoker)
        {
            var index = block.Statements.IndexOf(assignStatement);
            var previousStatement = assignStatement;
            for (var i = index + 1; i < block.Statements.Count; i++)
            {
                var statement = block.Statements[i] as ExpressionStatementSyntax;
                var invocation = statement?.Expression as InvocationExpressionSyntax;
                if (invocation == null)
                {
                    break;
                }

                var identifierName = invocation.Expression as IdentifierNameSyntax;
                if (identifierName == null)
                {
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (!(memberAccess?.Expression is ThisExpressionSyntax))
                    {
                        break;
                    }

                    identifierName = memberAccess.Name as IdentifierNameSyntax;
                }

                if (identifierName == null)
                {
                    break;
                }

                if (identifierName.Identifier.ValueText == invoker.Name)
                {
                    previousStatement = statement;
                }
            }

            return previousStatement;
        }
    }
}