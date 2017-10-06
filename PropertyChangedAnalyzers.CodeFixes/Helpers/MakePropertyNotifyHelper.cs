namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;

    internal static class MakePropertyNotifyHelper
    {
        internal static string BackingFieldNameForAutoProperty(PropertyDeclarationSyntax property, bool usesUnderscoreNames)
        {
            var typeDeclaration = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            var fieldName = usesUnderscoreNames
                              ? $"_{property.Identifier.ValueText.ToFirstCharLower()}"
                              : property.Identifier.ValueText.ToFirstCharLower();
            while (typeDeclaration.HasMember(fieldName))
            {
                fieldName += "_";
            }

            return fieldName;
        }

        internal static TypeDeclarationSyntax WithBackingField(
            this TypeDeclarationSyntax typeDeclaration,
            SyntaxGenerator syntaxGenerator,
            FieldDeclarationSyntax field,
            PropertyDeclarationSyntax forProperty)
        {
            if (field == null)
            {
                return typeDeclaration;
            }

            if (typeDeclaration.Members.TryGetLast(x => x.IsKind(SyntaxKind.FieldDeclaration), out MemberDeclarationSyntax existsingMember))
            {
                FieldDeclarationSyntax before = null;
                FieldDeclarationSyntax after = null;
                PropertyDeclarationSyntax property = null;
                foreach (var member in typeDeclaration.Members)
                {
                    var otherProperty = member as PropertyDeclarationSyntax;
                    if (otherProperty == null)
                    {
                        continue;
                    }

                    if (otherProperty.Identifier.ValueText == forProperty.Identifier.ValueText)
                    {
                        property = otherProperty;
                        continue;
                    }

                    if (Property.TryGetBackingField(otherProperty, out IdentifierNameSyntax _, out FieldDeclarationSyntax fieldDeclaration))
                    {
                        if (property == null)
                        {
                            before = fieldDeclaration;
                        }
                        else
                        {
                            after = fieldDeclaration;
                        }
                    }
                }

                if (before != null)
                {
                    return typeDeclaration.InsertNodesAfter(before, new[] { field });
                }

                if (after != null)
                {
                    return typeDeclaration.InsertNodesBefore(after, new[] { field });
                }

                return typeDeclaration.InsertNodesAfter(existsingMember, new[] { field });
            }

            if (typeDeclaration.Members.TryGetFirst(
                x =>
                    x.IsKind(SyntaxKind.ConstructorDeclaration) ||
                    x.IsKind(SyntaxKind.EventFieldDeclaration) ||
                    x.IsKind(SyntaxKind.PropertyDeclaration) ||
                    x.IsKind(SyntaxKind.MethodDeclaration),
                out existsingMember))
            {
                return typeDeclaration.InsertNodesBefore(existsingMember, new[] { field });
            }

            return (TypeDeclarationSyntax)syntaxGenerator.AddMembers(typeDeclaration, field);
        }

        internal static PropertyDeclarationSyntax WithoutInitializer(this PropertyDeclarationSyntax property)
        {
            if (property.Initializer == null)
            {
                return property;
            }

            return property.WithInitializer(null)
                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                           .WithAccessorList(
                               property.AccessorList.WithCloseBraceToken(
                                   property
                                       .AccessorList
                                       .CloseBraceToken
                                       .WithTrailingTrivia(
                                           property
                                               .SemicolonToken
                                               .TrailingTrivia)));
        }

        internal static PropertyDeclarationSyntax WithGetterReturningBackingField(this PropertyDeclarationSyntax property, SyntaxGenerator syntaxGenerator, string field)
        {
            var fieldAccess = field.StartsWith("_")
                                    ? field
                                    : $"this.{field}";
            return WithGetterReturningBackingField(property, syntaxGenerator, SyntaxFactory.ParseExpression(fieldAccess));
        }

        internal static PropertyDeclarationSyntax WithGetterReturningBackingField(this PropertyDeclarationSyntax property, SyntaxGenerator syntaxGenerator, ExpressionSyntax fieldAccess)
        {
            var returnStatement = syntaxGenerator.ReturnStatement(fieldAccess);
            return (PropertyDeclarationSyntax)syntaxGenerator.WithGetAccessorStatements(property, new[] { returnStatement }).WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static PropertyDeclarationSyntax WithNotifyingSetter(
            this PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property,
            SyntaxGenerator syntaxGenerator,
            string field,
            IMethodSymbol invoker,
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            return WithNotifyingSetter(
                propertyDeclaration,
                property,
                syntaxGenerator,
                syntaxGenerator.AssignValueToBackingField(field),
                field,
                invoker,
                diagnosticOptions);
        }

        internal static PropertyDeclarationSyntax WithNotifyingSetter(
            this PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property,
            SyntaxGenerator syntaxGenerator,
            ExpressionStatementSyntax assign,
            string field,
            IMethodSymbol invoker,
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            var propertyName = propertyDeclaration.Identifier.ValueText;
            var statements = new[]
                                 {
                                     syntaxGenerator.IfValueEqualsBackingFieldReturn(field, property, diagnosticOptions),
                                     assign.WithTrailingTrivia(SyntaxFactory.ElasticMarker),
                                     syntaxGenerator.OnPropertyChanged(
                                         propertyName: propertyName,
                                         useCallerMemberName: invoker.IsCallerMemberName(),
                                         usedUnderscoreNames: field.StartsWith("_"),
                                         invoker: invoker),
                                 };
            return (PropertyDeclarationSyntax)syntaxGenerator.WithSetAccessorStatements(propertyDeclaration, statements)
                                                             .WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static PropertyDeclarationSyntax WithNotifyingSetter(
            this PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property,
            SyntaxGenerator syntaxGenerator,
            ExpressionStatementSyntax assign,
            ExpressionSyntax fieldAccess,
            IMethodSymbol invoker,
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            var propertyName = propertyDeclaration.Identifier.ValueText;
            var statements = new[]
                             {
                                 syntaxGenerator.IfValueEqualsBackingFieldReturn(fieldAccess, property, diagnosticOptions),
                                 assign.WithTrailingTrivia(SyntaxFactory.ElasticMarker),
                                 syntaxGenerator.OnPropertyChanged(
                                     propertyName: propertyName,
                                     useCallerMemberName: invoker.IsCallerMemberName(),
                                     usedUnderscoreNames: fieldAccess.UsesUnderscoreNames(null, CancellationToken.None),
                                     invoker: invoker),
                             };
            return (PropertyDeclarationSyntax)syntaxGenerator.WithSetAccessorStatements(propertyDeclaration, statements)
                                                             .WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static StatementSyntax OnPropertyChanged(this SyntaxGenerator syntaxGenerator, string propertyName, bool useCallerMemberName, bool usedUnderscoreNames, IMethodSymbol invoker)
        {
            var prefix = usedUnderscoreNames
                             ? string.Empty
                             : "this.";
            if (invoker == null)
            {
                var eventAccess = SyntaxFactory.ParseExpression($"{prefix}PropertyChanged?.Invoke(new PropertyChangedEventArgs(nameof({propertyName}))");
                return (StatementSyntax)syntaxGenerator.ExpressionStatement(eventAccess.WithAdditionalAnnotations(Formatter.Annotation))
                                                       .WithAdditionalAnnotations(Formatter.Annotation);
            }

            var memberAccess = SyntaxFactory.ParseExpression($"{prefix}{invoker.Name}");
            if (useCallerMemberName && invoker.Parameters[0].IsCallerMemberName())
            {
                return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess));
            }

            var arg = SyntaxFactory.ParseExpression($"nameof({prefix}{propertyName})").WithAdditionalAnnotations(Formatter.Annotation);
            return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess, arg))
                                                   .WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static IfStatementSyntax IfValueEqualsBackingFieldReturn(this SyntaxGenerator syntaxGenerator, string fieldName, IPropertySymbol property, ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            var fieldAccess = fieldName.StartsWith("_")
                ? fieldName
                : $"this.{fieldName}";
            return IfValueEqualsBackingFieldReturn(
                syntaxGenerator,
                SyntaxFactory.ParseExpression(fieldAccess),
                property,
                diagnosticOptions);
        }

        internal static IfStatementSyntax IfValueEqualsBackingFieldReturn(this SyntaxGenerator syntaxGenerator, ExpressionSyntax fieldAccess, IPropertySymbol property, ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            if (!property.Type.IsReferenceType ||
                property.Type == KnownSymbol.String)
            {
                if (Equality.HasEqualityOperator(property.Type))
                {
                    var valueEqualsExpression = syntaxGenerator.ValueEqualsExpression(
                        SyntaxFactory.ParseName("value"),
                        fieldAccess);
                    return (IfStatementSyntax)syntaxGenerator.IfStatement(
                        valueEqualsExpression,
                        new[] { SyntaxFactory.ReturnStatement() });
                }

                foreach (var equals in property.Type.GetMembers("Equals"))
                {
                    var method = equals as IMethodSymbol;
                    if (method?.Parameters.Length == 1 &&
                        ReferenceEquals(
                            method.Parameters[0]
                                  .Type,
                            property.Type))
                    {
                        var equalsExpression = syntaxGenerator.InvocationExpression(
                            SyntaxFactory.ParseExpression("value.Equals"),
                            fieldAccess);
                        return (IfStatementSyntax)syntaxGenerator.IfStatement(
                            equalsExpression,
                            new[] { SyntaxFactory.ReturnStatement() });
                    }
                }

                if (property.Type.Name == "Nullable")
                {
                    if (Equality.HasEqualityOperator(((INamedTypeSymbol)property.Type).TypeArguments[0]))
                    {
                        var valueEqualsExpression = syntaxGenerator.ValueEqualsExpression(
                            SyntaxFactory.ParseName("value"),
                            fieldAccess);
                        return (IfStatementSyntax)syntaxGenerator.IfStatement(
                            valueEqualsExpression,
                            new[] { SyntaxFactory.ReturnStatement() });
                    }

                    var nullableEquals = syntaxGenerator.InvocationExpression(
                        SyntaxFactory.ParseExpression("System.Nullable.Equals")
                                     .WithAdditionalAnnotations(Simplifier.Annotation),
                        SyntaxFactory.ParseName("value"),
                        fieldAccess);
                    return (IfStatementSyntax)syntaxGenerator.IfStatement(
                        nullableEquals,
                        new[] { SyntaxFactory.ReturnStatement() });
                }

                var comparerEquals = syntaxGenerator.InvocationExpression(
                    SyntaxFactory.ParseExpression(
                                     $"System.Collections.Generic.EqualityComparer<{property.Type.ToDisplayString()}>.Default.Equals")
                                 .WithAdditionalAnnotations(Simplifier.Annotation),
                    SyntaxFactory.ParseName("value"),
                    fieldAccess);
                return (IfStatementSyntax)syntaxGenerator.IfStatement(
                    comparerEquals,
                    new[] { SyntaxFactory.ReturnStatement() });
            }

            var referenceEqualsExpression = syntaxGenerator.InvocationExpression(
                ReferenceTypeEquality(diagnosticOptions),
                SyntaxFactory.ParseName("value"),
                fieldAccess);
            return (IfStatementSyntax)syntaxGenerator.IfStatement(
                referenceEqualsExpression,
                new[] { SyntaxFactory.ReturnStatement() });
        }

        [Obsolete("Use snippet")]
        internal static ExpressionSyntax ReferenceTypeEquality(ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            if (!diagnosticOptions.TryGetValue(INPC006UseReferenceEquals.DiagnosticId, out ReportDiagnostic setting))
            {
                return SyntaxFactory.ParseExpression(nameof(ReferenceEquals));
            }

            return setting == ReportDiagnostic.Suppress
                ? SyntaxFactory.ParseExpression(nameof(Equals))
                : SyntaxFactory.ParseExpression(nameof(ReferenceEquals));
        }

        private static ExpressionStatementSyntax AssignValueToBackingField(this SyntaxGenerator syntaxGenerator, string fieldName)
        {
            var fieldAccess = fieldName.StartsWith("_")
                                  ? fieldName
                                  : $"this.{fieldName}";

            var assignmentStatement = syntaxGenerator.AssignmentStatement(SyntaxFactory.ParseExpression(fieldAccess), SyntaxFactory.ParseName("value"));
            return (ExpressionStatementSyntax)syntaxGenerator.ExpressionStatement(assignmentStatement);
        }

        private static bool HasMember(this TypeDeclarationSyntax typeDeclaration, string name)
        {
            foreach (var member in typeDeclaration.Members)
            {
                if (member is BaseFieldDeclarationSyntax fieldDeclaration)
                {
                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        if (variable.Identifier.ValueText == name)
                        {
                            return true;
                        }
                    }

                    continue;
                }

                if (member is PropertyDeclarationSyntax property)
                {
                    if (property.Identifier.ValueText == name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}