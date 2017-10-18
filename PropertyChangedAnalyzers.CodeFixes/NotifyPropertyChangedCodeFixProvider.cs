namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyPropertyChangedCodeFixProvider))]
    [Shared]
    internal class NotifyPropertyChangedCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC003NotifyWhenPropertyChanges.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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
                            if (invocation.Parent is ExpressionStatementSyntax ||
                                invocation.Parent is ArrowExpressionClauseSyntax)
                            {
                                context.RegisterCodeFix(
                                    new DocumentEditorAction(
                                        $"Notify that property {property} changes.",
                                        context.Document,
                                        (editor, cancellationToken) => MakeNotifyCreateIf(
                                            editor,
                                            invocation,
                                            property,
                                            invoker,
                                            usesUnderscoreNames),
                                        this.GetType()
                                            .Name),
                                    diagnostic);
                                continue;
                            }

                            if (invocation.Parent is IfStatementSyntax ifStatement)
                            {
                                context.RegisterCodeFix(
                                    new DocumentEditorAction(
                                        $"Notify that property {property} changes.",
                                        context.Document,
                                        (editor, _) => MakeNotifyInIf(
                                            editor,
                                            ifStatement,
                                            property,
                                            invoker,
                                            usesUnderscoreNames),
                                        this.GetType()
                                            .Name),
                                    diagnostic);
                                continue;
                            }

                            if (invocation.Parent is PrefixUnaryExpressionSyntax unary &&
                                unary.IsKind(SyntaxKind.LogicalNotExpression) &&
                                unary.Parent is IfStatementSyntax ifStatement2 &&
                                ifStatement2.IsReturnOnly())
                            {
                                context.RegisterCodeFix(
                                    new DocumentEditorAction(
                                        $"Notify that property {property} changes.",
                                        context.Document,
                                        (editor, _) => MakeNotify(
                                            editor,
                                            expression,
                                            property,
                                            invoker,
                                            usesUnderscoreNames),
                                        this.GetType().Name),
                                    diagnostic);
                                continue;
                            }
                        }

                        context.RegisterCodeFix(
                            new DocumentEditorAction(
                                $"Notify that property {property} changes.",
                                context.Document,
                                (editor, _) => MakeNotify(
                                    editor,
                                    expression,
                                    property,
                                    invoker,
                                    usesUnderscoreNames),
                                this.GetType().Name),
                            diagnostic);
                    }
                }
            }
        }

        private static void MakeNotify(DocumentEditor editor, ExpressionSyntax assignment, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames)
        {
            var onPropertyChanged = SyntaxFactory.ParseStatement(Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames))
                                                 .WithSimplifiedNames()
                                                 .WithLeadingElasticLineFeed()
                                                 .WithTrailingElasticLineFeed()
                                                 .WithAdditionalAnnotations(Formatter.Annotation);
            if (assignment.Parent is AnonymousFunctionExpressionSyntax anonymousFunction)
            {
                if (anonymousFunction.Body is BlockSyntax block)
                {
                    if (block.Statements.Count > 1)
                    {
                        var previousStatement = InsertAfter(block, block.Statements.Last(), invoker);
                        editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
                    }

                    return;
                }

                var expressionStatement = (ExpressionStatementSyntax)editor.Generator.ExpressionStatement(anonymousFunction.Body);
                var withStatements = editor.Generator.WithStatements(anonymousFunction, new[] { expressionStatement, onPropertyChanged });
                editor.ReplaceNode(anonymousFunction, withStatements);
            }
            else if (assignment.Parent is ExpressionStatementSyntax assignStatement &&
                     assignStatement.Parent is BlockSyntax assignBlock)
            {
                var previousStatement = InsertAfter(assignBlock, assignStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
            }
            else if (assignment.Parent is PrefixUnaryExpressionSyntax unary &&
                     unary.IsKind(SyntaxKind.LogicalNotExpression) &&
                     unary.Parent is IfStatementSyntax ifStatement &&
                     ifStatement.Parent is BlockSyntax ifBlock)
            {
                var previousStatement = InsertAfter(ifBlock, ifStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
            }
        }

        private static void MakeNotifyCreateIf(DocumentEditor editor, InvocationExpressionSyntax invocation, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames)
        {
            if (invocation.Parent is ExpressionStatementSyntax assignStatement &&
                assignStatement.Parent is BlockSyntax)
            {
                editor.ReplaceNode(
                    assignStatement,
                    (node, _) =>
                    {
                        var code = StringBuilderPool.Borrow()
                                                    .AppendLine($"if ({invocation.ToFullString().TrimEnd('\r', '\n')})")
                                                    .AppendLine("{")
                                                    .AppendLine($"    {Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                                    .AppendLine("}")
                                                    .Return();

                        return SyntaxFactory.ParseStatement(code)
                                            .WithSimplifiedNames()
                                            .WithLeadingElasticLineFeed()
                                            .WithTrailingElasticLineFeed()
                                            .WithAdditionalAnnotations(Formatter.Annotation);
                    });
                editor.FormatNode(invocation.FirstAncestorOrSelf<PropertyDeclarationSyntax>());
                return;
            }

            if (invocation.Parent is ArrowExpressionClauseSyntax arrow &&
                arrow.Parent is AccessorDeclarationSyntax accessor)
            {
                editor.RemoveNode(accessor.ExpressionBody);
                editor.ReplaceNode(
                    accessor,
                    x =>
                    {
                        var code = StringBuilderPool.Borrow()
                                                    .AppendLine($"if ({invocation.ToFullString().TrimEnd('\r', '\n')})")
                                                    .AppendLine("{")
                                                    .AppendLine($"    {Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                                    .AppendLine("}")
                                                    .Return();

                        var body = SyntaxFactory.ParseStatement(code)
                                                .WithSimplifiedNames()
                                                .WithLeadingElasticLineFeed()
                                                .WithTrailingElasticLineFeed()
                                                .WithAdditionalAnnotations(Formatter.Annotation);
                        return x.WithBody(SyntaxFactory.Block(body))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
                    });
                editor.FormatNode(invocation.FirstAncestorOrSelf<PropertyDeclarationSyntax>());
            }
        }

        private static void MakeNotifyInIf(DocumentEditor editor, IfStatementSyntax ifStatement, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames)
        {
            var onPropertyChanged = SyntaxFactory.ParseStatement(Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames))
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
                    return;
                }

                var previousStatement = InsertAfter(block, ifStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
                return;
            }

            if (ifStatement.Statement != null)
            {
                editor.ReplaceNode(
                    ifStatement.Statement,
                    (node, _) =>
                    {
                        var code = StringBuilderPool.Borrow()
                                                    .AppendLine("{")
                                                    .AppendLine($"{ifStatement.Statement.ToFullString().TrimEnd('\r', '\n')}")
                                                    .AppendLine($"    {Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                                    .AppendLine("}")
                                                    .Return();

                        return SyntaxFactory.ParseStatement(code)
                                            .WithSimplifiedNames()
                                            .WithTrailingElasticLineFeed()
                                            .WithAdditionalAnnotations(Formatter.Annotation);
                    });
            }
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