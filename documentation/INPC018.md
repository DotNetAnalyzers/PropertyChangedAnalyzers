# INPC018
## PropertyChanged invoker should be protected when the class is not sealed.

| Topic    | Value
| :--      | :--
| Id       | INPC018
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [MethodDeclarationAnalyzer]([MethodDeclarationAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/MethodDeclarationAnalyzer.cs))


## Description

PropertyChanged invoker should be protected when the class is not sealed.

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
#pragma warning disable INPC018 // PropertyChanged invoker should be protected when the class is not sealed.
Code violating the rule here
#pragma warning restore INPC018 // PropertyChanged invoker should be protected when the class is not sealed.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC018 // PropertyChanged invoker should be protected when the class is not sealed.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC018:PropertyChanged invoker should be protected when the class is not sealed.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->