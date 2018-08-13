# INPC008
## Struct must not implement INotifyPropertyChanged

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC008</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>Error</td>
  </tr>
  <tr>
    <td>Enabled</td>
    <td>true</td>
  </tr>
  <tr>
    <td>Category</td>
    <td>PropertyChangedAnalyzers.PropertyChanged</td>
  </tr>
  <tr>
    <td>Code</td>
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/INPC008StructMustNotNotify.cs">INPC008StructMustNotNotify</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Struct must not implement INotifyPropertyChanged

## Motivation

ADD MOTIVATION HERE

## How to fix violations

ADD HOW TO FIX VIOLATIONS HERE

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC008 // Struct must not implement INotifyPropertyChanged
Code violating the rule here
#pragma warning restore INPC008 // Struct must not implement INotifyPropertyChanged
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC008 // Struct must not implement INotifyPropertyChanged
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC008:Struct must not implement INotifyPropertyChanged", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->