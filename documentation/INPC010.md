# INPC010
## The property gets and sets a different backing member.

| Topic    | Value
| :--      | :--
| Id       | INPC010
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)

## Description

The property gets and sets a different backing member.

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
#pragma warning disable INPC010 // The property gets and sets a different backing member.
Code violating the rule here
#pragma warning restore INPC010 // The property gets and sets a different backing member.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC010 // The property gets and sets a different backing member.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC010:The property gets and sets a different backing member.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->