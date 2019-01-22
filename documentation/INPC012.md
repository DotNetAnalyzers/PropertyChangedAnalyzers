# INPC012
## Don't use expression for raising PropertyChanged.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC012</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>Warning</td>
  </tr>
  <tr>
    <td>Enabled</td>
    <td>True</td>
  </tr>
  <tr>
    <td>Category</td>
    <td>PropertyChangedAnalyzers.PropertyChanged</td>
  </tr>
  <tr>
    <td>Code</td>
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/NodeAnalyzers/ArgumentAnalyzer.cs">ArgumentAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Don't use expression for raising PropertyChanged.

## Motivation

Wasteful for no reason given `nameof` and `[CallerMemberName]` were added to the language.

## How to fix violations

Use `nameof` or `[CallerMemberName]` to extract the name to notify for.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC012 // Don't use expression for raising PropertyChanged.
Code violating the rule here
#pragma warning restore INPC012 // Don't use expression for raising PropertyChanged.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC012 // Don't use expression for raising PropertyChanged.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC012:Don't use expression for raising PropertyChanged.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->