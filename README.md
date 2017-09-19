# PropertyChangedAnalyzers

[![Join the chat at https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers](https://badges.gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/PropertyChangedAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/25nvar8j6evtmtg4/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/PropertyChangedAnalyzers-twfog/branch/master)
[![NuGet](https://img.shields.io/nuget/v/PropertyChangedAnalyzers.svg)](https://www.nuget.org/packages/PropertyChangedAnalyzers/)

Roslyn analyzers for WPF.

<!-- start generated table -->
<table>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0001.md">WPF0001</a></td>
  <td>Backing field for a DependencyProperty should match registered name.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0002.md">WPF0002</a></td>
  <td>Backing field for a DependencyPropertyKey should match registered name.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0003.md">WPF0003</a></td>
  <td>CLR property for a DependencyProperty should match registered name.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0004.md">WPF0004</a></td>
  <td>CLR method for a DependencyProperty should match registered name.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0005.md">WPF0005</a></td>
  <td>Name of PropertyChangedCallback should match registered name.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0006.md">WPF0006</a></td>
  <td>Name of CoerceValueCallback should match registered name.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0007.md">WPF0007</a></td>
  <td>Name of ValidateValueCallback should match registered name.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0010.md">WPF0010</a></td>
  <td>Default value type must match registered type.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0011.md">WPF0011</a></td>
  <td>Containing type should be used as registered owner.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0012.md">WPF0012</a></td>
  <td>CLR property type should match registered type.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0013.md">WPF0013</a></td>
  <td>CLR accessor for attached property must match registered type.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0014.md">WPF0014</a></td>
  <td>SetValue must use registered type.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0015.md">WPF0015</a></td>
  <td>Registered owner type must inherit DependencyObject.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0016.md">WPF0016</a></td>
  <td>Default value is shared reference type.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0030.md">WPF0030</a></td>
  <td>Backing field for a DependencyProperty should be static and readonly.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0031.md">WPF0031</a></td>
  <td>DependencyPropertyKey field must come before DependencyProperty field.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0032.md">WPF0032</a></td>
  <td>Use same dependency property in get and set.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0040.md">WPF0040</a></td>
  <td>A readonly DependencyProperty must be set with DependencyPropertyKey.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0041.md">WPF0041</a></td>
  <td>Set mutable dependency properties using SetCurrentValue.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0042.md">WPF0042</a></td>
  <td>Avoid side effects in CLR accessors.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0043.md">WPF0043</a></td>
  <td>Don't set DataContext using SetCurrentValue.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0050.md">WPF0050</a></td>
  <td>XmlnsPrefix must map to the same url as XmlnsDefinition.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0051.md">WPF0051</a></td>
  <td>XmlnsDefinition must map to existing namespace.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/WPF0052.md">WPF0052</a></td>
  <td>XmlnsDefinitions does not map all namespaces with public types.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC008.md">INPC008</a></td>
  <td>Struct must not implement INotifyPropertyChanged</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC002.md">INPC002</a></td>
  <td>Mutable public property should notify.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC003.md">INPC003</a></td>
  <td>Implement INotifyPropertyChanged.</td>
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
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC007.md">INPC007</a></td>
  <td>Don't raise PropertyChanged for missing property.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC005.md">INPC005</a></td>
  <td>Check if value is different before notifying.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC006.md">INPC006</a></td>
  <td>Check if value is different using ReferenceEquals before notifying.</td>
</tr>
<tr>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/tree/master/documentation/INPC006.md">INPC006</a></td>
  <td>Check if value is different using object.Equals before notifying.</td>
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