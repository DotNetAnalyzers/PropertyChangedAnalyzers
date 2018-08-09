# INPC007
## The class has PropertyChangedEvent but no invoker.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC007</td>
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
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers.Analyzers/INPC007MissingInvoker.cs">INPC007MissingInvoker</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

The class has PropertyChangedEvent but no invoker.

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
#pragma warning disable INPC007 // The class has PropertyChangedEvent but no invoker.
Code violating the rule here
#pragma warning restore INPC007 // The class has PropertyChangedEvent but no invoker.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC007 // The class has PropertyChangedEvent but no invoker.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC007:The class has PropertyChangedEvent but no invoker.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->