# INPC022
## The change is already notified for.

| Topic    | Value
| :--      | :--
| Id       | INPC022
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [InvocationAnalyzer]([InvocationAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/InvocationAnalyzer.cs))

## Description

The change is already notified for.

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
#pragma warning disable INPC022 // The change is already notified for.
Code violating the rule here
#pragma warning restore INPC022 // The change is already notified for.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC022 // The change is already notified for.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC022:The change is already notified for.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->