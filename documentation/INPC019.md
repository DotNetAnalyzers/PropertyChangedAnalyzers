# INPC019
## Getter should return backing field.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC019</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>Info</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/NodeAnalyzers/PropertyDeclarationAnalyzer.cs">PropertyDeclarationAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Getter should return backing field.

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
#pragma warning disable INPC019 // Getter should return backing field.
Code violating the rule here
#pragma warning restore INPC019 // Getter should return backing field.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC019 // Getter should return backing field.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC019:Getter should return backing field.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->