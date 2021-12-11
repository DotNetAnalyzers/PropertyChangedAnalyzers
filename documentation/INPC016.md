# INPC016
## Notify after update

| Topic    | Value
| :--      | :--
| Id       | INPC016
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)

## Description

Notify after updating the backing field.

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
#pragma warning disable INPC016 // Notify after update
Code violating the rule here
#pragma warning restore INPC016 // Notify after update
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC016 // Notify after update
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC016:Notify after update", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->