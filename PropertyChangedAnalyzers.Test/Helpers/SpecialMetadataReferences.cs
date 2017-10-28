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
        internal static MetadataReference Stylet { get; } = CreateStyletReference("Stylet.dll");

        /// <summary>
        /// This is needed as stylet is not signed.
        /// </summary>
        internal static MetadataReference MvvmCross { get; } = CreateStyletReference("MvvmCross.Core.dll");

        private static MetadataReference CreateStyletReference(string dllName)
        {
            var dll = CodeFactory.FindSolutionFile("PropertyChangedAnalyzers.sln")
                                 .Directory.EnumerateFiles(dllName, SearchOption.AllDirectories)
                                 .First();
            return MetadataReference.CreateFromFile(dll.FullName);
        }
    }
}