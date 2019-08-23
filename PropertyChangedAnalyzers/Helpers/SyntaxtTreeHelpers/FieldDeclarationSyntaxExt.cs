namespace PropertyChangedAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldDeclarationSyntaxExt
    {
        internal static string Name(this FieldDeclarationSyntax declaration)
        {
            if (declaration?.Declaration is VariableDeclarationSyntax variableDeclaration &&
                variableDeclaration.Variables.TrySingle(out var variable))
            {
                return variable.Identifier.Text;
            }

            throw new InvalidOperationException($"Could not get name of field {declaration}");
        }
    }
}
