# INPC001
## Implement INotifyPropertyChanged.

<!-- start generated table -->
<table>
<tr>
  <td>CheckId</td>
  <td>INPC003</td>
</tr>
<tr>
  <td>Severity</td>
  <td>Warning</td>
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
  <td>TypeName</td>
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers.Analyzers/PropertyChanged/INPC003ImplementINotifyPropertyChanged.cs">INPC003ImplementINotifyPropertyChanged</a></td>
</tr>
</table>
<!-- end generated table -->

## Description

Implement INotifyPropertyChanged.

## Motivation

This nag is helpful in finding and fixing places where we have forgotten to implement `INotifyPropertyChanged`

## How to fix violations

Use the code fix.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC003 // Implement INotifyPropertyChanged.
Code violating the rule here
#pragma warning restore INPC003 // Implement INotifyPropertyChanged.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC003 // Implement INotifyPropertyChanged.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC003:Implement INotifyPropertyChanged.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->