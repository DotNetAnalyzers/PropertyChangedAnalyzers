# INPC015
## Property is recursive.

| Topic    | Value
| :--      | :--
| Id       | INPC015
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [PropertyDeclarationAnalyzer]([PropertyDeclarationAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/NodeAnalyzers/PropertyDeclarationAnalyzer.cs))

## Description

Property is recursive.

## Motivation

Detects silly mistakes like:

```cs
public class Foo
{
    public int Bar => this.Bar;
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
