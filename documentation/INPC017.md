# INPC017
## Backing field name must match.

| Topic    | Value
| :--      | :--
| Id       | INPC017
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [PropertyDeclarationAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/PropertyDeclarationAnalyzer.cs)

## Description

Backing field name must match.

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
#pragma warning disable INPC017 // Backing field name must match.
Code violating the rule here
#pragma warning restore INPC017 // Backing field name must match.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC017 // Backing field name must match.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC017:Backing field name must match.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->