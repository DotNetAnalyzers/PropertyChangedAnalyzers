# INPC023
## Don't use instance equals in setter.

| Topic    | Value
| :--      | :--
| Id       | INPC023
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)


## Description

Instance equals could throw NullReferenceException.

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
#pragma warning disable INPC023 // Don't use instance equals in setter.
Code violating the rule here
#pragma warning restore INPC023 // Don't use instance equals in setter.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC023 // Don't use instance equals in setter.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC023:Don't use instance equals in setter.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->