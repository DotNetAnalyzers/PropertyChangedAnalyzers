namespace PropertyChangedAnalyzers.Test
{
    using System.IO;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;

    internal class SpecialMetadataReferences
    {
        /// <summary>
        /// This is needed as stylet is not signed.
        /// </summary>
        internal static MetadataReference Stylet { get; } = CreateStyletReference();

        private static MetadataReference CreateStyletReference()
        {
            var dll = CodeFactory.FindSolutionFile("PropertyChangedAnalyzers.sln")
                                      .Directory.EnumerateFiles("Stylet.dll", SearchOption.AllDirectories)
                                      .First();
            return MetadataReference.CreateFromFile(dll.FullName);
        }
    }
}