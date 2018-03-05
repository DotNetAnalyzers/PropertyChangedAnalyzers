# PropertyChangedAnalyzers

[![Join the chat at https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers](https://badges.gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/0d5ipb8hm82eiqmi/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/propertychangedanalyzers/branch/master)
[![NuGet](https://img.shields.io/nuget/v/PropertyChangedAnalyzers.svg)](https://www.nuget.org/packages/PropertyChangedAnalyzers/)

Roslyn analyzers for WPF.
* 1.x versions are for Visual Studio 2015.
* 2.x versions are for Visual Studio 2017.

![inpc](https://user-images.githubusercontent.com/1640096/33793418-2345625c-dcb8-11e7-9170-a5c0e778abc9.gif)

<!-- start generated table -->
<table>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC001.md">INPC001</a></td>
  <td>Implement INotifyPropertyChanged.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC002.md">INPC002</a></td>
  <td>Mutable public property should notify.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC003.md">INPC003</a></td>
  <td>Notify when property changes.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC004.md">INPC004</a></td>
  <td>Use [CallerMemberName]</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC005.md">INPC005</a></td>
  <td>Check if value is different before notifying.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC006_a.md">INPC006_a</a></td>
  <td>Check if value is different using ReferenceEquals before notifying.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC006_b.md">INPC006_b</a></td>
  <td>Check if value is different using object.Equals before notifying.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC007.md">INPC007</a></td>
  <td>The class has PropertyChangedEvent but no invoker.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC008.md">INPC008</a></td>
  <td>Struct must not implement INotifyPropertyChanged</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC009.md">INPC009</a></td>
  <td>Don't raise PropertyChanged for missing property.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC010.md">INPC010</a></td>
  <td>The property sets a different field than it returns.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC011.md">INPC011</a></td>
  <td>Don't shadow PropertyChanged event.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC012.md">INPC012</a></td>
  <td>Don't use expression for raising PropertyChanged.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC013.md">INPC013</a></td>
  <td>Use nameof.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC014.md">INPC014</a></td>
  <td>Prefer setting backing field in constructor.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC015.md">INPC015</a></td>
  <td>Property is recursive.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC016.md">INPC016</a></td>
  <td>Notify after update.</td>
</tr>
<table>
<!-- end generated table -->

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
