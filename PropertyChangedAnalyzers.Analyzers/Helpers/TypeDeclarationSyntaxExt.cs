namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeDeclarationSyntaxExt
    {
        internal static bool ImplementsINotifyPropertyChanged(this TypeDeclarationSyntax typeDeclaration)
        {
            var types = typeDeclaration?.BaseList?.Types;
            if (types == null)
            {
                return false;
            }

            foreach (var type in types.Value)
            {
                if (type == KnownSymbol.INotifyPropertyChanged)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
