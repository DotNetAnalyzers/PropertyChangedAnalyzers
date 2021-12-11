# INPC001
## The class has mutable properties and should implement INotifyPropertyChanged

| Topic    | Value
| :--      | :--
| Id       | INPC001
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [ClassDeclarationAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/ClassDeclarationAnalyzer.cs)

## Description

The class has mutable properties and should implement INotifyPropertyChanged.
The default severity is warning meaning there will be a squiggle and a build warning.
Setting severity to `Hidden`turns it into a refactoring. Then only the code fix shows up on the class but no warning.
![ImplementInpc](https://user-images.githubusercontent.com/1640096/63153711-44091880-c00f-11e9-9185-564c52575b4a.gif)

## Motivation

This nag is helpful for finding and fixing places where we have forgotten to implement `INotifyPropertyChanged`

## How to fix violations

Use the code fix.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC001 // The class has mutable properties and should implement INotifyPropertyChanged
Code violating the rule here
#pragma warning restore INPC001 // The class has mutable properties and should implement INotifyPropertyChanged
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC001 // The class has mutable properties and should implement INotifyPropertyChanged
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC001:The class has mutable properties and should implement INotifyPropertyChanged", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
