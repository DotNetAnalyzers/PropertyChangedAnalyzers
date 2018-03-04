#pragma warning disable 660,661 // using a hack with operator overloads
namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    // ReSharper disable once UseNameofExpression
    [System.Diagnostics.DebuggerDisplay("{FullName}")]
    internal class QualifiedType
    {
        internal readonly string FullName;
        internal readonly NamespaceParts Namespace;
        internal readonly string Type;

        internal QualifiedType(string qualifiedName)
            : this(qualifiedName, NamespaceParts.Create(qualifiedName), qualifiedName.Substring(qualifiedName.LastIndexOf('.') + 1))
        {
        }

        private QualifiedType(string fullName, NamespaceParts @namespace, string type)
        {
            this.FullName = fullName;
            this.Namespace = @namespace;
            this.Type = type;
        }

        public static bool operator ==(ITypeSymbol left, QualifiedType right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.MetadataName == right.Type &&
                   left.ContainingNamespace == right.Namespace;
        }

        public static bool operator !=(ITypeSymbol left, QualifiedType right) => !(left == right);

        public static bool operator ==(BaseTypeSyntax left, QualifiedType right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Type == right;
        }

        public static bool operator !=(BaseTypeSyntax left, QualifiedType right) => !(left == right);

        public static bool operator ==(TypeSyntax left, QualifiedType right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left is SimpleNameSyntax simple)
            {
                return simple.Identifier.ValueText == right.Type;
            }

            if (left is QualifiedNameSyntax qualified)
            {
                return right.Namespace.Matches(qualified.Left);
            }

            return false;
        }

        public static bool operator !=(TypeSyntax left, QualifiedType right) => !(left == right);
    }
}
