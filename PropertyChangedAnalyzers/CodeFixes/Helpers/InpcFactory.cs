namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;

    internal static class InpcFactory
    {
        internal static readonly AttributeListSyntax CallerMemberNameAttributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(ParseQualifiedName("System.Runtime.CompilerServices.CallerMemberName"))));

        internal static readonly IdentifierNameSyntax Value = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("value"));

        internal static QualifiedNameSyntax ParseQualifiedName(string name) => (QualifiedNameSyntax)SyntaxFactory.ParseTypeName(name).WithAdditionalAnnotations(Simplifier.Annotation);

        internal static IfStatementSyntax IfReturn(ExpressionSyntax condition)
        {
            return SyntaxFactory.IfStatement(
                SyntaxFactory.Token(SyntaxKind.IfKeyword),
                SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                condition,
                SyntaxFactory.Token(SyntaxKind.CloseParenToken),
                SyntaxFactory.Block(SyntaxFactory.ReturnStatement()),
                null)
                                .WithTrailingLineFeed();
        }

        internal static IfStatementSyntax IfStatement(ExpressionSyntax condition, params StatementSyntax[] statements)
        {
            return SyntaxFactory.IfStatement(
                SyntaxFactory.Token(SyntaxKind.IfKeyword),
                SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                condition,
                SyntaxFactory.Token(SyntaxKind.CloseParenToken),
                SyntaxFactory.Block(statements),
                null);
        }

        internal static ExpressionSyntax Equals(ExpressionSyntax x, ExpressionSyntax y, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (TryGetType(out var type))
            {
                return Equals(type!, x.WithoutTrivia(), y.WithoutTrivia(), semanticModel);
            }

            throw new InvalidOperationException("Failed creating equality check.");

            bool TryGetType(out ITypeSymbol? result)
            {
                if (semanticModel.TryGetType(x, cancellationToken, out result) ||
                    semanticModel.TryGetType(y, cancellationToken, out result))
                {
                    return true;
                }

                if (semanticModel.TryGetSymbol(x, cancellationToken, out var symbol) ||
                    semanticModel.TryGetSymbol(y, cancellationToken, out symbol))
                {
                    switch (symbol)
                    {
                        case IParameterSymbol parameter:
                            result = parameter.Type;
                            return true;
                        case ILocalSymbol local:
                            result = local.Type;
                            return true;
                        case IFieldSymbol field:
                            result = field.Type;
                            return true;
                        case IPropertySymbol property:
                            result = property.Type;
                            return true;
                    }
                }

                return false;
            }
        }

        internal static ExpressionSyntax Equals(string? className, string methodName, ExpressionSyntax x, ExpressionSyntax y)
        {
            return SyntaxFactory.InvocationExpression(
                Expression(),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        new[]
                        {
                            SyntaxFactory.Argument(x),
                            SyntaxFactory.Argument(y),
                        })));

            ExpressionSyntax Expression()
            {
                return className switch
                {
                    null => (ExpressionSyntax)SyntaxFactory.IdentifierName(methodName),

                    "object" => SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ParseQualifiedName(className),
                                SyntaxFactory.IdentifierName(methodName)),

                    _ => SyntaxFactory.MemberAccessExpression(
                         SyntaxKind.SimpleMemberAccessExpression,
                         ParseQualifiedName(className),
                         SyntaxFactory.IdentifierName(methodName)),
                };
            }
        }

        internal static ExpressionSyntax Equals(ITypeSymbol type, ExpressionSyntax x, ExpressionSyntax y, SemanticModel semanticModel)
        {
            if (type == KnownSymbol.String)
            {
                return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, x, y);
            }

            if (!type.IsReferenceType)
            {
                if (Gu.Roslyn.AnalyzerExtensions.Equality.HasEqualityOperator(type))
                {
                    return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, x, y);
                }

                if (type == KnownSymbol.NullableOfT)
                {
                    if (Gu.Roslyn.AnalyzerExtensions.Equality.HasEqualityOperator(((INamedTypeSymbol)type).TypeArguments[0]))
                    {
                        return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, x, y);
                    }

                    return Equals("System.Nullable", nameof(Equals), x, y);
                }

                if (type.GetMembers(nameof(Equals))
                        .TrySingleOfType(m => m.Parameters.Length == 1 && Equals(type, m.Parameters[0].Type), out IMethodSymbol _))
                {
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            x,
                            SyntaxFactory.IdentifierName(nameof(Equals))),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(y))));
                }

                return Equals($"System.Collections.Generic.EqualityComparer<{type.ToDisplayString()}>.Default", nameof(Equals), x, y);
            }

            return Equals(null, Descriptors.INPC006UseReferenceEqualsForReferenceTypes.IsSuppressed(semanticModel) ? nameof(Equals) : nameof(ReferenceEquals), x, y);
        }

        internal static AccessorDeclarationSyntax AsExpressionBody(this AccessorDeclarationSyntax accessor, ExpressionSyntax expression)
        {
            return SyntaxFactory.AccessorDeclaration(
                                    kind: accessor.Kind(),
                                    attributeLists: accessor.AttributeLists,
                                    modifiers: accessor.Modifiers,
                                    keyword: accessor.Keyword.WithTrailingTrivia(SyntaxFactory.Space),
                                    body: default,
                                    expressionBody: SyntaxFactory.ArrowExpressionClause(expression),
                                    semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                .WithTriviaFrom(accessor);
        }

        internal static AccessorDeclarationSyntax AsBlockBody(this AccessorDeclarationSyntax accessor, params StatementSyntax[] statements)
        {
            return SyntaxFactory.AccessorDeclaration(
                                    kind: accessor.Kind(),
                                    attributeLists: accessor.AttributeLists,
                                    modifiers: accessor.Modifiers,
                                    keyword: accessor.Keyword,
                                    body: SyntaxFactory.Block(statements),
                                    expressionBody: default,
                                    semicolonToken: SyntaxFactory.Token(SyntaxKind.None))
                                .WithTriviaFrom(accessor);
        }

        internal static SimpleLambdaExpressionSyntax AsBlockBody(this SimpleLambdaExpressionSyntax lambda, params StatementSyntax[] statements)
        {
            return SyntaxFactory.SimpleLambdaExpression(
                asyncKeyword: lambda.AsyncKeyword,
                parameter: lambda.Parameter,
                arrowToken: SyntaxFactory.Token(lambda.ArrowToken.Kind()),
                body: SyntaxFactory.Block(statements)
                                   .WithLeadingLineFeed())
                                .WithTriviaFrom(lambda)
                                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static ParenthesizedLambdaExpressionSyntax AsBlockBody(this ParenthesizedLambdaExpressionSyntax lambda, params StatementSyntax[] statements)
        {
            return SyntaxFactory.ParenthesizedLambdaExpression(
                asyncKeyword: lambda.AsyncKeyword,
                parameterList: lambda.ParameterList,
                arrowToken: SyntaxFactory.Token(lambda.ArrowToken.Kind()),
                body: SyntaxFactory.Block(statements)
                                   .WithLeadingLineFeed())
                                .WithTriviaFrom(lambda)
                                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static InvocationExpressionSyntax TrySetInvocation(CodeStyleResult qualifyAccess, IMethodSymbol method, ExpressionSyntax fieldAccess, ExpressionSyntax value, ExpressionSyntax? name)
        {
            return SyntaxFactory.InvocationExpression(
                qualifyAccess == CodeStyleResult.No
                    ? (ExpressionSyntax)SyntaxFactory.IdentifierName(method.Name)
                    : SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(method.Name)),
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(Arguments())));

            IEnumerable<ArgumentSyntax> Arguments()
            {
                yield return SyntaxFactory.Argument(default, SyntaxFactory.Token(SyntaxKind.RefKeyword), fieldAccess.WithoutTrivia());
                yield return SyntaxFactory.Argument(value);
                if (name != null)
                {
                    yield return SyntaxFactory.Argument(name.WithoutTrivia());
                }
            }
        }

        internal static ExpressionStatementSyntax OnPropertyChangedInvocationStatement(ExpressionSyntax methodAccess, ExpressionSyntax? nameExpression)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    methodAccess,
                    Arguments()));

            ArgumentListSyntax Arguments()
            {
                if (nameExpression is null)
                {
                    return SyntaxFactory.ArgumentList();
                }

                return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(nameExpression)));
            }
        }

        internal static MethodDeclarationSyntax OnPropertyChangedDeclaration(CodeStyleResult qualifyAccess, bool isSealed, bool isStatic, bool callerMemberName)
        {
            return SyntaxFactory.MethodDeclaration(
                attributeLists: default,
                modifiers: Modifiers(),
                returnType: SyntaxFactory.PredefinedType(
                    keyword: SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                explicitInterfaceSpecifier: default,
                identifier: SyntaxFactory.Identifier("OnPropertyChanged"),
                typeParameterList: default,
                parameterList: SyntaxFactory.ParameterList(
                    openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    parameters: SyntaxFactory.SingletonSeparatedList(Parameter("propertyName")),
                    closeParenToken: SyntaxFactory.Token(
                        leading: default,
                        kind: SyntaxKind.CloseParenToken,
                        trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))),
                constraintClauses: default,
                body: SyntaxFactory.Block(
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ExpressionStatement(
                            expression: SyntaxFactory.ConditionalAccessExpression(
                                expression: SymbolAccess("PropertyChanged", qualifyAccess),
                                operatorToken: SyntaxFactory.Token(SyntaxKind.QuestionToken),
                                whenNotNull: SyntaxFactory.InvocationExpression(
                                    expression: SyntaxFactory.MemberBindingExpression(
                                        operatorToken: SyntaxFactory.Token(SyntaxKind.DotToken),
                                        name: SyntaxFactory.IdentifierName(
                                            identifier: SyntaxFactory.Identifier(
                                                leading: default,
                                                text: "Invoke",
                                                trailing: default))),
                                    argumentList: SyntaxFactory.ArgumentList(
                                        openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                        arguments: SyntaxFactory.SeparatedList(
                                            new[]
                                            {
                                                ThisArgument(),
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        newKeyword: SyntaxFactory.Token(
                                                            leading: default,
                                                            kind: SyntaxKind.NewKeyword,
                                                            trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                                        type: ParseQualifiedName("System.ComponentModel.PropertyChangedEventArgs"),
                                                        argumentList: SyntaxFactory.ArgumentList(
                                                            openParenToken: SyntaxFactory.Token(
                                                                SyntaxKind.OpenParenToken),
                                                            arguments: SyntaxFactory
                                                                .SingletonSeparatedList(
                                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("propertyName"))),
                                                            closeParenToken: SyntaxFactory.Token(
                                                                SyntaxKind.CloseParenToken)),
                                                        initializer: default)),
                                            }),
                                        closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken)))),
                            semicolonToken: SyntaxFactory.Token(
                                leading: default,
                                kind: SyntaxKind.SemicolonToken,
                                trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))))),
                expressionBody: default,
                semicolonToken: default);

            ArgumentSyntax ThisArgument()
            {
                return SyntaxFactory.Argument(
                    isStatic
                        ? (ExpressionSyntax)SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                        : SyntaxFactory.ThisExpression());
            }

            SyntaxTokenList Modifiers()
            {
                if (isStatic)
                {
                    return SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword));
                }

                return isSealed
                    ? SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    : SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                        SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
            }

            ParameterSyntax Parameter(string name)
            {
                return callerMemberName
                    ? InpcFactory.CallerMemberName(name)
                    : SyntaxFactory.Parameter(
                            attributeLists: default,
                            modifiers: default,
                            type: SyntaxFactory.PredefinedType(keyword: SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                            identifier: SyntaxFactory.Identifier(name),
                            @default: default);
            }
        }

        internal static ParameterSyntax CallerMemberName(string name)
        {
            return SyntaxFactory.Parameter(
                attributeLists: SyntaxFactory.SingletonList(CallerMemberNameAttributeList),
                modifiers: default,
                type: SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind: SyntaxKind.StringKeyword)),
                identifier: SyntaxFactory.Identifier(name),
                @default: SyntaxFactory.EqualsValueClause(
                    value: SyntaxFactory.LiteralExpression(
                        kind: SyntaxKind.NullLiteralExpression,
                        token: SyntaxFactory.Token(SyntaxKind.NullKeyword))));
        }

        /// <summary>
        /// Workaround for bug in roslyn: https://github.com/dotnet/roslyn/issues/24212.
        /// </summary>
        /// <param name="expression">The <see cref="ExpressionSyntax"/>.</param>
        /// <returns>A <see cref="InvocationExpressionSyntax"/>.</returns>
        internal static InvocationExpressionSyntax Nameof(ExpressionSyntax expression)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Identifier(
                        leading: default,
                        contextualKind: SyntaxKind.NameOfKeyword,
                        text: "nameof",
                        valueText: "nameof",
                        trailing: default)),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expression))));
        }

        internal static ExpressionSyntax SymbolAccess(string name, CodeStyleResult qualify)
        {
            if (name[0] == '@')
            {
                name = name.TrimStart('@');
            }

            var identifier = SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
                ? SyntaxFactory.VerbatimIdentifier(SyntaxTriviaList.Empty, name, name, SyntaxTriviaList.Empty)
                : SyntaxFactory.Identifier(name);

            if (qualify == CodeStyleResult.No)
            {
                return SyntaxFactory.IdentifierName(identifier);
            }

            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.Token(SyntaxKind.DotToken),
                SyntaxFactory.IdentifierName(identifier));
        }
    }
}
