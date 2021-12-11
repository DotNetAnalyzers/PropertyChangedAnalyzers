# INPC013
## Use nameof

| Topic    | Value
| :--      | :--
| Id       | INPC013
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [ArgumentAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/ArgumentAnalyzer.cs)

## Description

Use nameof.

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
#pragma warning disable INPC013 // Use nameof
Code violating the rule here
#pragma warning restore INPC013 // Use nameof
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC013 // Use nameof
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC013:Use nameof", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->