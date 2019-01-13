# INPC015
## Property is recursive.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC015</td>
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
    <td>Code</td>
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/NodeAnalyzers/PropertyDeclarationAnalyzer.cs">PropertyDeclarationAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Property is recursive.

## Motivation

Detects silly mistakes like:

```cs
public class Foo
{
    public int Bar => ↓this.Bar;
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC015 // Property is recursive.
Code violating the rule here
#pragma warning restore INPC015 // Property is recursive.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC015 // Property is recursive.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC015:Property is recursive.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
