namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullableFix))]
    [Shared]
    internal class NullableFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("CS8618", "CS8625");

        protected override DocumentEditorFixAllProvider? FixAllProvider() => DocumentEditorFixAllProvider.Solution;

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                           .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == "CS8618" &&
                    syntaxRoot is { } &&
                    semanticModel is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MemberDeclarationSyntax? member) &&
                    diagnostic.GetMessage(CultureInfo.InvariantCulture) is { } message)
                {
                    if (Regex.Match(message, "Non-nullable event '(?<name>[^']+)' is uninitialized") is { Success: true } eventMatch &&
                        eventMatch.Groups["name"].Value is { } eventName &&
                        FindEventType(member) is { } type)
                    {
                        context.RegisterCodeFix(
                            $"Declare {eventName} as nullable.",
                            (editor, _) => editor.ReplaceNode(
                                type,
                                x => SyntaxFactory.NullableType(x)),
                            "Declare event as nullable.",
                            diagnostic);
                    }

                    if (Regex.Match(message, "Non-nullable field '(?<name>[^']+)' is uninitialized") is { Success: true } fieldMatch &&
                        fieldMatch.Groups["name"].Value is { } fieldName &&
                        FindFieldType(member) is { } fieldType &&
                        FindProperty() is { } property)
                    {
                        context.RegisterCodeFix(
                            $"Declare field {fieldName} and property {property.Identifier.ValueText} as nullable.",
                            (editor, _) => editor.ReplaceNode(
                                                     fieldType,
                                                     x => SyntaxFactory.NullableType(x))
                                                 .ReplaceNode(
                                                     property.Type,
                                                     x => SyntaxFactory.NullableType(x)),
                            "Declare field and property as nullable.",
                            diagnostic);
                    }

                    TypeSyntax? FindFieldType(MemberDeclarationSyntax candidate)
                    {
                        return candidate switch
                        {
                            FieldDeclarationSyntax { Declaration: { Variables: { Count: 1 }, Type: { } t } }
                            when semanticModel.GetTypeInfo(t) is { Type: { IsReferenceType: true } }
                            => t,
                            ConstructorDeclarationSyntax { Parent: TypeDeclarationSyntax typeDeclaration }
                            when typeDeclaration.TryFindField(fieldName, out var field)
                            => FindFieldType(field),
                            _ => null,
                        };
                    }

                    TypeSyntax? FindEventType(MemberDeclarationSyntax candidate)
                    {
                        return candidate switch
                        {
                            EventDeclarationSyntax { Type: { } t } => t,
                            EventFieldDeclarationSyntax { Declaration: { Type: { } t } } => t,
                            ConstructorDeclarationSyntax { Parent: TypeDeclarationSyntax typeDeclaration }
                            when typeDeclaration.TryFindEvent(eventName, out member)
                            => FindEventType(member),
                            _ => null,
                        };
                    }

                    PropertyDeclarationSyntax? FindProperty()
                    {
                        if (member is { Parent: TypeDeclarationSyntax containingType })
                        {
                            foreach (var candidate in containingType.Members)
                            {
                                if (candidate is PropertyDeclarationSyntax property &&
                                    property.TrySingleReturned(out var returned))
                                {
                                    switch (returned)
                                    {
                                        case IdentifierNameSyntax identifierName
                                            when identifierName.Identifier.ValueText == fieldName:
                                            return property;
                                        case MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: IdentifierNameSyntax identifierName }
                                            when identifierName.Identifier.ValueText == fieldName:
                                            return property;
                                    }
                                }
                            }
                        }

                        return null;
                    }
                }
                else if (diagnostic.Id == "CS8625" &&
                         syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ParameterSyntax? parameter))
                {
                    context.RegisterCodeFix(
                        $"Declare {parameter.Identifier} as nullable.",
                        (editor, _) => editor.ReplaceNode(
                            parameter.Type,
                            x => SyntaxFactory.NullableType(x)),
                        "Declare parameter as nullable.",
                        diagnostic);
                }
            }
        }
    }
}
