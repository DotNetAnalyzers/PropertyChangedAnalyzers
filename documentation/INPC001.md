# INPC001
## Implement INotifyPropertyChanged.

| Topic    | Value
| :--      | :--
| Id       | INPC001
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [INPC001ImplementINotifyPropertyChanged]([INPC001ImplementINotifyPropertyChanged](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/INPC001ImplementINotifyPropertyChanged.cs))

## Description

Implement INotifyPropertyChanged.

## Motivation

This nag is helpful in finding and fixing places where we have forgotten to implement `INotifyPropertyChanged`

## How to fix violations

Use the code fix.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC001 // Implement INotifyPropertyChanged.
Code violating the rule here
#pragma warning restore INPC001 // Implement INotifyPropertyChanged.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC001 // Implement INotifyPropertyChanged.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC001:Implement INotifyPropertyChanged.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->