# INPC011
## Don't shadow PropertyChanged event.

| Topic    | Value
| :--      | :--
| Id       | INPC011
| Severity | Error
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [INPC011DontShadow](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/INPC011DontShadow.cs)

## Description

Don't shadow PropertyChanged event.

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
#pragma warning disable INPC011 // Don't shadow PropertyChanged event.
Code violating the rule here
#pragma warning restore INPC011 // Don't shadow PropertyChanged event.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC011 // Don't shadow PropertyChanged event.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC011:Don't shadow PropertyChanged event.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->