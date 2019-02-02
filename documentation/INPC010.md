# INPC010
## The property sets a different field than it returns.

| Topic    | Value
| :--      | :--
| Id       | INPC010
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [PropertyDeclarationAnalyzer]([PropertyDeclarationAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/NodeAnalyzers/PropertyDeclarationAnalyzer.cs))

## Description

The property sets a different field than it returns.

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
#pragma warning disable INPC010 // The property sets a different field than it returns.
Code violating the rule here
#pragma warning restore INPC010 // The property sets a different field than it returns.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC010 // The property sets a different field than it returns.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC010:The property sets a different field than it returns.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->