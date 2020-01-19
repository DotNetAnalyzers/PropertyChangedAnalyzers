# PropertyChangedAnalyzers

[![Join the chat at https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers](https://badges.gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/0d5ipb8hm82eiqmi/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/propertychangedanalyzers/branch/master)
[![Build Status](https://dev.azure.com/DotNetAnalyzers/PropertyChangedAnalyzers/_apis/build/status/DotNetAnalyzers.PropertyChangedAnalyzers?branchName=master)](https://dev.azure.com/DotNetAnalyzers/PropertyChangedAnalyzers/_build/latest?definitionId=3&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/PropertyChangedAnalyzers.svg)](https://www.nuget.org/packages/PropertyChangedAnalyzers/)

Roslyn analyzers for `INotifyPropertyChanged`.
* 1.x versions are for Visual Studio 2015.
* 2.x versions are for Visual Studio 2017.
* 3.x versions are for Visual Studio 2019.

![Animation](https://user-images.githubusercontent.com/1640096/72191376-3e6aff80-3402-11ea-8be4-881f8c6a531b.gif)

| Id       | Title
| :--      | :--
| [INPC001](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC001.md)| The class has mutable properties and should implement INotifyPropertyChanged.
| [INPC002](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC002.md)| Mutable public property should notify.
| [INPC003](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC003.md)| Notify when property changes.
| [INPC004](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC004.md)| Use [CallerMemberName]
| [INPC005](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC005.md)| Check if value is different before notifying.
| [INPC006_a](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC006_a.md)| Check if value is different using ReferenceEquals before notifying.
| [INPC006_b](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC006_b.md)| Check if value is different using object.Equals before notifying.
| [INPC007](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC007.md)| The class has PropertyChangedEvent but no invoker.
| [INPC008](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC008.md)| Struct must not implement INotifyPropertyChanged
| [INPC009](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC009.md)| Don't raise PropertyChanged for missing property.
| [INPC010](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC010.md)| The property gets and sets a different backing member.
| [INPC011](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC011.md)| Don't shadow PropertyChanged event.
| [INPC012](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC012.md)| Don't use expression for raising PropertyChanged.
| [INPC013](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC013.md)| Use nameof.
| [INPC014](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC014.md)| Prefer setting backing field in constructor.
| [INPC015](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC015.md)| Property is recursive.
| [INPC016](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC016.md)| Notify after update.
| [INPC017](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC017.md)| Backing field name must match.
| [INPC018](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC018.md)| PropertyChanged invoker should be protected when the class is not sealed.
| [INPC019](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC019.md)| Getter should return backing field.
| [INPC020](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC020.md)| Prefer expression body accessor.
| [INPC021](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC021.md)| Setter should set backing field.
| [INPC022](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC022.md)| Comparison should be with backing field.
| [INPC023](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC023.md)| Don't use instance equals in setter.

## Using PropertyChangedAnalyzers

The preferable way to use the analyzers is to add the nuget package [PropertyChangedAnalyzers](https://www.nuget.org/packages/PropertyChangedAnalyzers/)
to the project(s).

The severity of individual rules may be configured using [rule set files](https://msdn.microsoft.com/en-us/library/dd264996.aspx)
in Visual Studio 2015.

## Installation

PropertyChangedAnalyzers can be installed using [Paket](https://fsprojects.github.io/Paket/) or the NuGet command line or the NuGet Package Manager in Visual Studio 2015.


**Install using the command line:**
```bash
Install-Package PropertyChangedAnalyzers
```

## Updating

The ruleset editor does not handle changes IDs well, if things get out of sync you can try:

1) Close visual studio.
2) Edit the ProjectName.rulset file and remove the PropertyChangedAnalyzers element.
3) Start visual studio and add back the desired configuration.

Above is not ideal, sorry about this. Not sure this is our bug.

### When using old style packages.config
Many times running the nuget update command breaks things. The script below can be used to manually update.
```cs
var package = "PropertyChangedAnalyzers";
var oldVersion = "2.7.2.0";
var newVersion = "2.7.3";
foreach (var csproj in Directory.EnumerateFiles("C:\\Git\\Path\\To\Sln", "*.csproj", SearchOption.AllDirectories))
{
    string text = File.ReadAllText(csproj);
    if (text.Contains($"{package}.{oldVersion}"))
    {
        // <Analyzer Include="..\packages\PropertyChangedAnalyzers.2.7.2.0\analyzers\dotnet\cs\PropertyChangedAnalyzers.dll" />
        // <Analyzer Include="..\packages\PropertyChangedAnalyzers.2.7.2.0\analyzers\dotnet\cs\Gu.Roslyn.Extensions.dll" />
        File.WriteAllText(csproj, text.Replace($"{package}.{oldVersion}", $"{package}.{newVersion}"));
    }

    // <package id="PropertyChangedAnalyzers" version="2.7.2.0" targetFramework="net46" developmentDependency="true" />
}

foreach (var csproj in Directory.EnumerateFiles("C:\\Tfs\\Coromatic\\BoxEr", "packages.config", SearchOption.AllDirectories))
{
    string text = File.ReadAllText(csproj);
    if (text.Contains($"id=\"{package}\" version=\"{oldVersion}\""))
    {
        // <package id="PropertyChangedAnalyzers" version="2.7.2.0" targetFramework="net46" developmentDependency="true" />
        File.WriteAllText(csproj, text.Replace($"id=\"{package}\" version=\"{oldVersion}\"", $"id=\"{package}\" version=\"{newVersion}\""));
    }
}
```

## Current status

Early alpha, names and IDs may change.
