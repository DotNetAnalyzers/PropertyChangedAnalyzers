namespace PropertyChangedAnalyzers
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    internal static class DocumentEditorExt
    {
        internal static FieldDeclarationSyntax AddBackingField(this DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, bool usesUnderscoreNames, CancellationToken cancellationToken)
        {
            var property = editor.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
            var name = usesUnderscoreNames
                ? $"_{property.Name.ToFirstCharLower()}"
                : property.Name.ToFirstCharLower();
            while (property.ContainingType.MemberNames.Any(x => x == name))
            {
                name += "_";
            }

            var backingField = (FieldDeclarationSyntax)editor.Generator.FieldDeclaration(
                name,
                accessibility: Accessibility.Private,
                modifiers: DeclarationModifiers.None,
                type: propertyDeclaration.Type,
                initializer: propertyDeclaration.Initializer?.Value);
            var type = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            var index = type.Members.IndexOf(propertyDeclaration);
            for (var i = index + 1; i < type.Members.Count; i++)
            {
                if (type.Members[i] is PropertyDeclarationSyntax other)
                {
                    if (Property.TryGetBackingField(other, out _, out var field))
                    {
                        editor.InsertBefore(field, backingField);
                        return backingField;
                    }
                }
                else
                {
                    break;
                }
            }

            for (int i = index - 1; i >= 0; i--)
            {
                if (type.Members[i] is PropertyDeclarationSyntax other)
                {
                    if (Property.TryGetBackingField(other, out _, out var field))
                    {
                        editor.InsertBefore(field, backingField);
                        return backingField;
                    }
                }
                else
                {
                    break;
                }
            }

            editor.AddField(type, backingField);
            return backingField;
        }

        internal static DocumentEditor AddField(this DocumentEditor editor, TypeDeclarationSyntax containingType, FieldDeclarationSyntax field)
        {
            editor.ReplaceNode(containingType, (node, generator) => AddSorted(generator, (TypeDeclarationSyntax)node, field));
            return editor;
        }

        internal static DocumentEditor AddEvent(this DocumentEditor editor, ClassDeclarationSyntax containingType, EventDeclarationSyntax @event)
        {
            editor.ReplaceNode(containingType, (node, generator) => AddSorted(generator, (ClassDeclarationSyntax)node, @event));
            return editor;
        }

        internal static DocumentEditor AddEvent(this DocumentEditor editor, ClassDeclarationSyntax containingType, EventFieldDeclarationSyntax @event)
        {
            editor.ReplaceNode(containingType, (node, generator) => AddSorted(generator, (ClassDeclarationSyntax)node, @event));
            return editor;
        }

        internal static DocumentEditor AddMethod(this DocumentEditor editor, TypeDeclarationSyntax containingType, MethodDeclarationSyntax method)
        {
            editor.ReplaceNode(containingType, (node, generator) => AddSorted(generator, (TypeDeclarationSyntax)node, method));
            return editor;
        }

        private static SyntaxNode AddSorted(SyntaxGenerator generator, TypeDeclarationSyntax containingType, MemberDeclarationSyntax memberDeclaration)
        {
            var memberIndex = MemberIndex(memberDeclaration);
            for (var i = 0; i < containingType.Members.Count; i++)
            {
                var member = containingType.Members[i];
                if (memberIndex < MemberIndex(member))
                {
                    return generator.InsertMembers(containingType, i, memberDeclaration);
                }
            }

            return generator.AddMembers(containingType, memberDeclaration);
        }

        private static int MemberIndex(MemberDeclarationSyntax member)
        {
            int ModifierOffset(SyntaxTokenList modifiers)
            {
                if (modifiers.Any(SyntaxKind.ConstKeyword))
                {
                    return 0;
                }

                if (modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    if (modifiers.Any(SyntaxKind.ReadOnlyKeyword))
                    {
                        return 1;
                    }

                    return 2;
                }

                if (modifiers.Any(SyntaxKind.ReadOnlyKeyword))
                {
                    return 3;
                }

                return 4;
            }

            int AccessOffset(Accessibility accessibility)
            {
                const int step = 5;
                switch (accessibility)
                {
                    case Accessibility.Public:
                        return 0 * step;
                    case Accessibility.Internal:
                        return 1 * step;
                    case Accessibility.ProtectedAndInternal:
                        return 2 * step;
                    case Accessibility.ProtectedOrInternal:
                        return 3 * step;
                    case Accessibility.Protected:
                        return 4 * step;
                    case Accessibility.Private:
                        return 5 * step;
                    case Accessibility.NotApplicable:
                        return int.MinValue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null);
                }
            }

            Accessibility Accessability(SyntaxTokenList modifiers)
            {
                if (modifiers.Any(SyntaxKind.PublicKeyword))
                {
                    return Accessibility.Public;
                }

                if (modifiers.Any(SyntaxKind.InternalKeyword))
                {
                    return Accessibility.Internal;
                }

                if (modifiers.Any(SyntaxKind.ProtectedKeyword) &&
                    modifiers.Any(SyntaxKind.InternalKeyword))
                {
                    return Accessibility.ProtectedAndInternal;
                }

                if (modifiers.Any(SyntaxKind.ProtectedKeyword))
                {
                    return Accessibility.Protected;
                }

                if (modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    return Accessibility.Private;
                }

                return Accessibility.Private;
            }

            int TypeOffset(SyntaxKind kind)
            {
                const int step = 5 * 6;
                switch (kind)
                {
                    case SyntaxKind.FieldDeclaration:
                        return 0 * step;
                    case SyntaxKind.ConstructorDeclaration:
                        return 1 * step;
                    case SyntaxKind.EventDeclaration:
                    case SyntaxKind.EventFieldDeclaration:
                        return 2 * step;
                    case SyntaxKind.PropertyDeclaration:
                        return 3 * step;
                    case SyntaxKind.MethodDeclaration:
                        return 4 * step;
                    default:
                        return int.MinValue;
                }
            }

            var mfs = member.Modifiers();
            return TypeOffset(member.Kind()) + AccessOffset(Accessability(mfs)) + ModifierOffset(mfs);
        }

        private static SyntaxTokenList Modifiers(this MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    return field.Modifiers;
                case BasePropertyDeclarationSyntax prop:
                    return prop.Modifiers;
                case BaseMethodDeclarationSyntax method:
                    return method.Modifiers;
                case TypeDeclarationSyntax type:
                    return type.Modifiers;
                default:
                    return default(SyntaxTokenList);
            }
        }
    }
}