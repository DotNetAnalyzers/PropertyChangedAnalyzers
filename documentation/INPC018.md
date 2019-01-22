# INPC018
## PropertyChanged invoker should be protected when the class is not sealed.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC018</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/NodeAnalyzers/MethodDeclarationAnalyzer.cs">MethodDeclarationAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

PropertyChanged invoker should be protected when the class is not sealed.

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
#pragma warning disable INPC018 // PropertyChanged invoker should be protected when the class is not sealed.
Code violating the rule here
#pragma warning restore INPC018 // PropertyChanged invoker should be protected when the class is not sealed.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC018 // PropertyChanged invoker should be protected when the class is not sealed.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC018:PropertyChanged invoker should be protected when the class is not sealed.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->