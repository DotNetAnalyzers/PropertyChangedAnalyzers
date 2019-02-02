# INPC003
## Notify when property changes.

| Topic    | Value
| :--      | :--
| Id       | INPC003
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [INPC003NotifyWhenPropertyChanges]([INPC003NotifyWhenPropertyChanges](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/INPC003NotifyWhenPropertyChanges.cs))

## Description

Notify when property changes.

## Motivation

In the following example the setter for `FirstName` and `LastName` should notify about that the calculated property `FullName` changes.

```c#
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## How to fix violations

Use the code fix or manually change the code so that it notifies:

```c#
```c#
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC003 // Notify when property changes.
Code violating the rule here
#pragma warning restore INPC003 // Notify when property changes.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC003 // Notify when property changes.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC003:Notify when property changes.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->