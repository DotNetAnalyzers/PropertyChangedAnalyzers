# INPC020
## Prefer expression body accessor.

| Topic    | Value
| :--      | :--
| Id       | INPC020
| Severity | Info
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [PropertyDeclarationAnalyzer]([PropertyDeclarationAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/PropertyDeclarationAnalyzer.cs))

## Description

Prefer expression body accessor.

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
#pragma warning disable INPC020 // Prefer expression body accessor.
Code violating the rule here
#pragma warning restore INPC020 // Prefer expression body accessor.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC020 // Prefer expression body accessor.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC020:Prefer expression body accessor.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->