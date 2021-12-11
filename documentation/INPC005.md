# INPC005
## Check if value is different before notifying

| Topic    | Value
| :--      | :--
| Id       | INPC005
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)

## Description

Check if value is different before notifying.

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
#pragma warning disable INPC005 // Check if value is different before notifying
Code violating the rule here
#pragma warning restore INPC005 // Check if value is different before notifying
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC005 // Check if value is different before notifying
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC005:Check if value is different before notifying", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->