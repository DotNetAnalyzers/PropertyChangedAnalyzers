namespace PropertyChangedAnalyzers
{
    internal class MicrosoftPracticesPrismMvvmBindableBase : QualifiedType
    {
        internal readonly QualifiedMethod SetProperty;
        internal readonly QualifiedMethod OnPropertyChanged;
        internal readonly QualifiedMethod OnPropertyChangedOfT;

        internal MicrosoftPracticesPrismMvvmBindableBase()
            : base("Microsoft.Practices.Prism.Mvvm.BindableBase")
        {
            this.SetProperty = new QualifiedMethod(this, nameof(this.SetProperty));
            this.OnPropertyChanged = new QualifiedMethod(this, nameof(this.OnPropertyChanged));
            this.OnPropertyChangedOfT = new QualifiedMethod(this, $"{nameof(this.OnPropertyChanged)}`1");
        }
    }
}