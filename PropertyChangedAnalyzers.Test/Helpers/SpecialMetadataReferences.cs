namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Collections.Immutable;
    using System.IO;
    using System.Reflection;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;

    public static class SpecialMetadataReferences
    {
        private static readonly DirectoryInfo ProjectDirectory = ProjectFile.Find("PropertyChangedAnalyzers.Test.csproj").Directory;

        internal static ImmutableArray<MetadataReference> CaliburnMicro { get; } = MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase)).ToImmutableArray();

        internal static ImmutableArray<MetadataReference> Stylet { get; } = MetadataReferences.Transitive(LoadUnsigned("Stylet.dll")).ToImmutableArray();

        internal static ImmutableArray<MetadataReference> MvvmLight { get; } = MetadataReferences.Transitive(typeof(GalaSoft.MvvmLight.ViewModelBase)).ToImmutableArray();

        internal static ImmutableArray<MetadataReference> MvvmCross { get; } = MetadataReferences.Transitive(LoadUnsigned("MvvmCross.dll")).ToImmutableArray();

        internal static ImmutableArray<MetadataReference> Prism { get; } = MetadataReferences.Transitive(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase)).ToImmutableArray();

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
