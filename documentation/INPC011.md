# INPC011
## Don't shadow PropertyChanged event.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC011</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers.Analyzers/INPC011DontShadow.cs">INPC011DontShadow</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Don't shadow PropertyChanged event.

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
#pragma warning disable INPC011 // Don't shadow PropertyChanged event.
Code violating the rule here
#pragma warning restore INPC011 // Don't shadow PropertyChanged event.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC011 // Don't shadow PropertyChanged event.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC011:Don't shadow PropertyChanged event.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->