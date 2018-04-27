namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;

    internal class SpecialMetadataReferences
    {
        private static readonly DirectoryInfo ProjectDirectory = ProjectFile.Find("PropertyChangedAnalyzers.Test.csproj").Directory;

        /// <summary>
        /// This is needed as stylet is not signed.
        /// </summary>
        internal static MetadataReference Stylet { get; } = CreateDllReference("Stylet.dll");

        /// <summary>
        /// This is needed as MvvmCross is not signed.
        /// </summary>
        internal static MetadataReference MvvmCross { get; } = CreateDllReference("MvvmCross.dll");

        internal static IReadOnlyList<MetadataReference> MvvmCrossReferences { get; } = CreateMvvmCrossReferences();

        private static MetadataReference CreateDllReference(string dllName)
        {
            // ReSharper disable once PossibleNullReferenceException
            if (ProjectDirectory.EnumerateFiles(dllName, SearchOption.AllDirectories)
                               .TryFirst(out var dll))
            {
                return MetadataReference.CreateFromFile(dll.FullName);
            }

            throw new FileNotFoundException(dllName);
        }

        private static IReadOnlyList<MetadataReference> CreateMvvmCrossReferences()
        {
            return new[]
                   {
                       MvvmCross,
                       CreateDllReference("System.Runtime.dll"),
                       CreateDllReference("netstandard.dll"),
                       CreateDllReference("System.Linq.Expressions.dll"),
                       CreateDllReference("System.ObjectModel.dll"),
                   };
        }
    }
}
