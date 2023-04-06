namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChanged;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class CS0246
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0246");

        [Test]
        public static void WhenInterfaceOnlyAddUsingsNullableDisable()
        {
            var before = """
                #nullable disable
                namespace N
                {
                    public class C : ↓INotifyPropertyChanged
                    {
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

                        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.");
        }

        [Test]
        public static void WhenInterfaceOnlyAddUsings()
        {
            var before = """
                namespace N
                {
                    public class C : ↓INotifyPropertyChanged
                    {
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

                        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.");
        }

        [Test]
        public static void WhenInterfaceOnlyFullyQualified()
        {
            var before = """
                namespace N
                {
                    public class C : ↓INotifyPropertyChanged
                    {
                    }
                }
                """;

            var after = """
                namespace N
                {
                    public class C : System.ComponentModel.INotifyPropertyChanged
                    {
                        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

                        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenInterfaceOnlySealedAddUsings()
        {
            var before = """
                namespace N
                {
                    public sealed class C : ↓INotifyPropertyChanged
                    {
                    }
                }
                """;

            var after = """
                namespace N
                {
                    using System.ComponentModel;
                    using System.Runtime.CompilerServices;

                    public sealed class C : INotifyPropertyChanged
                    {
                        public event PropertyChangedEventHandler? PropertyChanged;

                        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.");
        }

        [Test]
        public static void WhenInterfaceOnlySealedFullyQualified()
        {
            var before = """
                namespace N
                {
                    public sealed class C : ↓INotifyPropertyChanged
                    {
                    }
                }
                """;

            var after = """
                namespace N
                {
                    public sealed class C : System.ComponentModel.INotifyPropertyChanged
                    {
                        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

                        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
                        {
                            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                        }
                    }
                }
                """;
            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }
    }
}
