# PropertyChangedAnalyzers

[![Join the chat at https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers](https://badges.gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/0d5ipb8hm82eiqmi/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/propertychangedanalyzers/branch/master)
[![NuGet](https://img.shields.io/nuget/v/PropertyChangedAnalyzers.svg)](https://www.nuget.org/packages/PropertyChangedAnalyzers/)

Roslyn analyzers for `INotifyPropertyChanged`.
* 1.x versions are for Visual Studio 2015.
* 2.x versions are for Visual Studio 2017.

![inpc](https://user-images.githubusercontent.com/1640096/33793418-2345625c-dcb8-11e7-9170-a5c0e778abc9.gif)

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
| [INPC010](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC010.md)| The property sets a different field than it returns.
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
| [INPC022](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC022.md)| The change is already notified for.

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


## Current status

Early alpha, names and IDs may change.
