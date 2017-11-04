# INPC014
## Prefer setting backing field in constructor.

<!-- start generated table -->
<table>
<tr>
  <td>CheckId</td>
  <td>INPC014</td>
</tr>
<tr>
  <td>Severity</td>
  <td>Info</td>
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
  <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers.Analyzers/INPC014PreferSettingBackingFieldInCtor.cs">INPC014PreferSettingBackingFieldInCtor</a></td>
</tr>
</table>
<!-- end generated table -->

## Description

Prefer setting backing field in constructor.

## Motivation

Setting the property often means a virtual `OnPropertyChangedCall` that can trigger warnings in other analyzers.
Setting the field also has slightly better performance.

Remarks:
There may be desired side effects in the setter, this analyzer does not check for that so it is up to you to decide if setting the backing field is right.

## How to fix violations

Use the code fix or change manually.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC014 // Prefer setting backing field in constructor.
Code violating the rule here
#pragma warning restore INPC014 // Prefer setting backing field in constructor.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC014 // Prefer setting backing field in constructor.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC014:Prefer setting backing field in constructor.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->