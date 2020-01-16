# INPC022
## Comparison should be with backing field.

| Topic    | Value
| :--      | :--
| Id       | INPC022
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)


## Description

Comparison should be with backing field.

## Motivation

```cs
public int P
{
    get => this.p;
    set
    {
        if (value == ↓this.f)
        {
            return;
        }

        this.p = value;
        this.OnPropertyChanged();
    }
}
```

In the above example equality is checked vs field `f` when field `p` is later assigned.

## How to fix violations

The code fix offers 

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC022 // Comparison should be with backing field.
Code violating the rule here
#pragma warning restore INPC022 // Comparison should be with backing field.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC022 // Comparison should be with backing field.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC022:Comparison should be with backing field.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->