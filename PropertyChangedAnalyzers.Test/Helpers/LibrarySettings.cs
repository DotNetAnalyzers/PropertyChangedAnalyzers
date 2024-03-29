﻿namespace PropertyChangedAnalyzers.Test.Helpers;

using System.IO;
using System.Reflection;
using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.Asserts;

public static class LibrarySettings
{
    private static readonly DirectoryInfo ProjectDirectory = ProjectFile.Find("PropertyChangedAnalyzers.Test.csproj").Directory;

    internal static Settings CaliburnMicro { get; } = Settings.Default
                                                              .WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS1701"))
                                                              .WithMetadataReferences(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase)));

    internal static Settings Stylet { get; } = Settings.Default
                                                       .WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS1701"))
                                                       .WithMetadataReferences(MetadataReferences.Transitive(LoadUnsigned("Stylet.dll")));

    internal static Settings MvvmLight { get; } = Settings.Default
                                                          .WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS1701"))
                                                          .WithMetadataReferences(MetadataReferences.Transitive(typeof(GalaSoft.MvvmLight.ViewModelBase)));

    internal static Settings MvvmCross { get; } = Settings.Default
                                                          .WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS1701"))
                                                          .WithMetadataReferences(MetadataReferences.Transitive(LoadUnsigned("MvvmCross.dll")));

    internal static Settings Prism { get; } = Settings.Default
                                                      .WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS1701"))
                                                      .WithMetadataReferences(MetadataReferences.Transitive(typeof(Microsoft.Practices.Prism.Mvvm.BindableBase)));

    internal static Settings Reactive { get; } = Settings.Default
                                                         .WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS1701"))
                                                         .WithMetadataReferences(MetadataReferences.Transitive(typeof(Gu.Wpf.Reactive.AsyncCommand)));

    internal static Settings Roslyn { get; } = Settings.Default
                                                       .WithMetadataReferences(MetadataReferences.Transitive(typeof(Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider)));

    // Use this if the dll is not signed
    private static Assembly LoadUnsigned(string dllName)
    {
        if (ProjectDirectory.EnumerateFiles(dllName, SearchOption.AllDirectories)
                            .TryFirst(out var dll))
        {
            return Assembly.LoadFile(dll.FullName);
        }

        throw new FileNotFoundException(dllName);
    }
}
