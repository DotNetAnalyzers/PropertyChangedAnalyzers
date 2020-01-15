# INPC021
## Setter should set backing field.

| Topic    | Value
| :--      | :--
| Id       | INPC021
| Severity | Info
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)

## Description

Setter should set backing field.

## Motivation

In the sample below not assigning the backing field is likely a bug.

```cs
public int P
{
    get => this.p;
    set
    {
        if (value == this.p)
        {
            return;
        }

        // this.p = value;
        this.OnPropertyChanged();
    }
}
```

## How to fix violations

Assign the backing member.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC021 // Setter should set backing field.
Code violating the rule here
#pragma warning restore INPC021 // Setter should set backing field.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC021 // Setter should set backing field.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC021:Setter should set backing field.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->