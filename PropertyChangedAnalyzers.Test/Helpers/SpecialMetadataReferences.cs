namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;

    public static class SpecialMetadataReferences
    {
        private static readonly DirectoryInfo ProjectDirectory = ProjectFile.Find("PropertyChangedAnalyzers.Test.csproj").Directory;

        internal static IReadOnlyList<MetadataReference> Stylet { get; } = MetadataReferences.Transitive(LoadUnsigned("Stylet.dll")).ToArray();

        internal static IReadOnlyList<MetadataReference> MvvmCross { get; } = MetadataReferences.Transitive(LoadUnsigned("MvvmCross.dll")).ToArray();

        // Use this if the dll is not signed
        private static Assembly LoadUnsigned(string dllName)
        {
            if (ProjectDirectory.EnumerateFiles(dllName, SearchOption.AllDirectories)
                                .TryFirst(out var dll))
            {
                return Assembly.ReflectionOnlyLoadFrom(dll.FullName);
            }

            throw new FileNotFoundException(dllName);
        }
    }
}
