namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;

    internal class SpecialMetadataReferences
    {
        /// <summary>
        /// This is needed as stylet is not signed.
        /// </summary>
        internal static MetadataReference Stylet { get; } = CreateDllReference("Stylet.dll");

        /// <summary>
        /// This is needed as MvvmCross is not signed.
        /// </summary>
        internal static MetadataReference MvvmCross { get; } = CreateDllReference("MvvmCross.Core.dll");

        internal static MetadataReference MvvmCrossPlatform { get; } = CreateDllReference("MvvmCross.Platform.dll");

        internal static IReadOnlyList<MetadataReference> MvvmCrossReferences { get; } = CreateMvvmCrossReferences();

        internal static IReadOnlyList<MetadataReference> AvaloniaReferences { get; } = CreateAvaloniaReferences();

        private static MetadataReference CreateDllReference(string dllName)
        {
            // ReSharper disable once PossibleNullReferenceException
            var dll = CodeFactory.FindSolutionFile("PropertyChangedAnalyzers.sln")
                                 .Directory.EnumerateFiles(dllName, SearchOption.AllDirectories)
                                 .First();
            return MetadataReference.CreateFromFile(dll.FullName);
        }

        private static IReadOnlyList<MetadataReference> CreateMvvmCrossReferences()
        {
            return new[]
                   {
                       MvvmCross,
                       MvvmCrossPlatform,
                       CreateDllReference("netstandard.dll"),
                       CreateDllReference("System.Runtime.dll"),
                       CreateDllReference("System.Linq.Expressions.dll"),
                       CreateDllReference("System.ObjectModel.dll"),
                   };
        }

        private static IReadOnlyList<MetadataReference> CreateAvaloniaReferences()
        {
            return new[]
                   {
                       CreateDllReference("Avalonia.Base.dll"),
                       CreateDllReference("netstandard.dll"),
                       CreateDllReference("System.Runtime.dll"),
                       CreateDllReference("System.Linq.Expressions.dll"),
                       CreateDllReference("System.ObjectModel.dll"),
                       CreateDllReference("System.Reactive.Core.dll"),
                       CreateDllReference("System.Reactive.Interfaces.dll"),
                       CreateDllReference("System.Reactive.Linq.dll"),
                   };
        }
    }
}
