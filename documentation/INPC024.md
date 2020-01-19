# INPC024
## ReferenceEquals is always false for value types.

| Topic    | Value
| :--      | :--
| Id       | INPC024
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)


## Description

ReferenceEquals is always false for value types.

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
#pragma warning disable INPC024 // ReferenceEquals is always false for value types.
Code violating the rule here
#pragma warning restore INPC024 // ReferenceEquals is always false for value types.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC024 // ReferenceEquals is always false for value types.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC024:ReferenceEquals is always false for value types.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->