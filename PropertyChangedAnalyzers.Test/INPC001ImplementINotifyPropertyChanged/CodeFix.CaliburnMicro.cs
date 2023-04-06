namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChanged;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public static partial class CodeFix
{
    public static class CaliburnMicro
    {
        private static readonly Settings Settings = LibrarySettings.CaliburnMicro;

        [Test]
        public static void SubclassPropertyChangedBaseAddUsing()
        {
            var before = """
                #nullable disable
                namespace N
                {
                    public class ↓C
                    {
                        public int P { get; set; }
                    }
                }
                """;

            var after = """
                #nullable disable
                namespace N
                {
                    using Caliburn.Micro;

                    public class C : PropertyChangedBase
                    {
                        public int P { get; set; }
                    }
                }
                """;
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Subclass Caliburn.Micro.PropertyChangedBase and add using.", settings: Settings);
        }

        [Test]
        public static void SubclassPropertyChangedBaseAddUsingNullableDisabled()
        {
            var before = """
                namespace N
                {
                    public class ↓C
                    {
                        public int P { get; set; }
                    }
                }
                """;

            var after = """
                namespace N
                {
                    using Caliburn.Micro;

                    public class C : PropertyChangedBase
                    {
                        public int P { get; set; }
                    }
                }
                """;
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Subclass Caliburn.Micro.PropertyChangedBase and add using.", settings: Settings);
        }

        [Test]
        public static void SubclassPropertyChangedBaseFullyQualified()
        {
            var before = """
                namespace N
                {
                    public class ↓C
                    {
                        public int P { get; set; }
                    }
                }
                """;

            var after = """
                namespace N
                {
                    public class C : Caliburn.Micro.PropertyChangedBase
                    {
                        public int P { get; set; }
                    }
                }
                """;
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Subclass Caliburn.Micro.PropertyChangedBase fully qualified.", settings: Settings);
        }

        [Test]
        public static void ImplementINotifyPropertyChangedAddUsingsNullableDisabled()
        {
            var before = """
                #nullable disable
                namespace N
                {
                    public class ↓C
                    {
                        public int P { get; set; }
                    }
                }
                """;

            var after = """
                #nullable disable
                namespace N
                {
                    using System.ComponentModel;
                    using System.Runtime.CompilerServices;

                    public class C : INotifyPropertyChanged
                    {
                        public event PropertyChangedEventHandler PropertyChanged;

                        public int P { get; set; }

                        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.", settings: Settings);
        }

        [Test]
        public static void ImplementINotifyPropertyChangedAddUsings()
        {
            var before = """
                namespace N
                {
                    public class ↓C
                    {
                        public int P { get; set; }
                    }
                }
                """;

            var after = """
                namespace N
                {
                    using System.ComponentModel;
                    using System.Runtime.CompilerServices;

                    public class C : INotifyPropertyChanged
                    {
                        public event PropertyChangedEventHandler? PropertyChanged;

                        public int P { get; set; }

                        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.", settings: Settings);
        }

        [Test]
        public static void ImplementINotifyPropertyChangedFullyQualifiedNullableDisabled()
        {
            var before = """
                #nullable disable
                namespace N
                {
                    public class ↓C
                    {
                        public int P { get; set; }
                    }
                }
                """;

            var after = """
                #nullable disable
                namespace N
                {
                    public class C : System.ComponentModel.INotifyPropertyChanged
                    {
                        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

                        public int P { get; set; }

                        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.", settings: Settings);
        }

        [Test]
        public static void ImplementINotifyPropertyChangedFullyQualified()
        {
            var before = """
                namespace N
                {
                    public class ↓C
                    {
                        public int P { get; set; }
                    }
                }
                """;

            var after = """
                namespace N
                {
                    public class C : System.ComponentModel.INotifyPropertyChanged
                    {
                        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

                        public int P { get; set; }

                        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.", settings: Settings);
        }
    }
}
