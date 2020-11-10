namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementINotifyPropertyChangedFix))]
    [Shared]
    internal class ImplementINotifyPropertyChangedFix : DocumentEditorCodeFixProvider
    {
        // ReSharper disable once InconsistentNaming
        private static readonly QualifiedNameSyntax INotifyPropertyChangedType = InpcFactory.ParseQualifiedName("System.ComponentModel.INotifyPropertyChanged");
        private static readonly QualifiedNameSyntax CallerMemberNameType = InpcFactory.ParseQualifiedName("System.Runtime.CompilerServices.CallerMemberName");
        private static readonly QualifiedNameSyntax PropertyChangedEventHandlerType = InpcFactory.ParseQualifiedName("System.ComponentModel.PropertyChangedEventHandler");
        private static readonly QualifiedNameSyntax MvvmLightViewModelBaseType = InpcFactory.ParseQualifiedName("GalaSoft.MvvmLight.ViewModelBase");
        private static readonly QualifiedNameSyntax CaliburnMicroPropertyChangedBase = InpcFactory.ParseQualifiedName("Caliburn.Micro.PropertyChangedBase");
        private static readonly QualifiedNameSyntax StyletPropertyChangedBase = InpcFactory.ParseQualifiedName("Stylet.PropertyChangedBase");
        private static readonly QualifiedNameSyntax MvvmCrossMvxNotifyPropertyChanged = InpcFactory.ParseQualifiedName("MvvmCross.ViewModels.MvxNotifyPropertyChanged");
        private static readonly QualifiedNameSyntax MvvmCrossMvxViewModel = InpcFactory.ParseQualifiedName("MvvmCross.ViewModels.MvxViewModel");
        private static readonly QualifiedNameSyntax MvvmCrossCoreMvxNotifyPropertyChanged = InpcFactory.ParseQualifiedName("MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged");
        private static readonly QualifiedNameSyntax MvvmCrossCoreMvxViewModel = InpcFactory.ParseQualifiedName("MvvmCross.Core.ViewModels.MvxViewModel");
        private static readonly QualifiedNameSyntax PrismMvvmBindableBase = InpcFactory.ParseQualifiedName("Microsoft.Practices.Prism.Mvvm.BindableBase");

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC001ImplementINotifyPropertyChanged.Id,
            "CS0535",
            "CS0246");

        protected override DocumentEditorFixAllProvider? FixAllProvider() => null;

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (IsSupportedDiagnostic(diagnostic) &&
                    syntaxRoot is { } &&
                    semanticModel is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ClassDeclarationSyntax? classDeclaration) &&
                    semanticModel.TryGetNamedType(classDeclaration, context.CancellationToken, out var type))
                {
                    if (type.BaseType == KnownSymbol.Object &&
                        !type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, semanticModel.Compilation))
                    {
                        if (References("GalaSoft.MvvmLight.dll"))
                        {
                            RegisterSubclassFixes(MvvmLightViewModelBaseType);
                        }

                        if (References("Caliburn.Micro.dll"))
                        {
                            RegisterSubclassFixes(CaliburnMicroPropertyChangedBase);
                        }

                        if (References("Stylet.dll"))
                        {
                            RegisterSubclassFixes(StyletPropertyChangedBase);
                        }

                        if (References("MvvmCross.dll"))
                        {
                            RegisterSubclassFixes(MvvmCrossMvxNotifyPropertyChanged);
                            RegisterSubclassFixes(MvvmCrossMvxViewModel);
                        }

                        if (References("MvvmCross.Core.dll"))
                        {
                            RegisterSubclassFixes(MvvmCrossCoreMvxNotifyPropertyChanged);
                            RegisterSubclassFixes(MvvmCrossCoreMvxViewModel);
                        }

                        if (References("Prism.Mvvm.dll"))
                        {
                            RegisterSubclassFixes(PrismMvvmBindableBase);
                        }

                        void RegisterSubclassFixes(QualifiedNameSyntax viewModelBaseType)
                        {
                            context.RegisterCodeFix(
                                $"Subclass {viewModelBaseType} and add using.",
                                (editor, _) =>
                                    Subclass(editor, addUsings: true),
                                $"Subclass {viewModelBaseType} and add usings.",
                                diagnostic);

                            context.RegisterCodeFix(
                                $"Subclass {viewModelBaseType} fully qualified.",
                                (editor, _) =>
                                    Subclass(editor, addUsings: false),
                                $"Subclass {viewModelBaseType} fully qualified.",
                                diagnostic);

                            void Subclass(DocumentEditor editor, bool addUsings)
                            {
                                if (classDeclaration!.BaseList is { Types: { } types } &&
                                    types.TryFirst(x => x.Type == KnownSymbol.INotifyPropertyChanged, out var baseType))
                                {
                                    _ = editor.ReplaceNode(
                                        baseType,
                                        x => BaseType().WithTriviaFrom(x));
                                }
                                else
                                {
                                    editor.AddBaseType(classDeclaration, BaseType());
                                }

                                if (addUsings)
                                {
                                    _ = editor.AddUsing(SyntaxFactory.UsingDirective(viewModelBaseType.Left));
                                }

                                TypeSyntax BaseType()
                                {
                                    return addUsings
                                        ? (TypeSyntax)viewModelBaseType.Right
                                        : viewModelBaseType;
                                }
                            }
                        }
                    }

                    context.RegisterCodeFix(
                        "Implement INotifyPropertyChanged and add usings.",
                        (editor, cancellationToken) =>
                            ImplementINotifyPropertyChangedAsync(
                                editor,
                                addUsings: true,
                                cancellationToken),
                        "Implement INotifyPropertyChanged and add usings.",
                        diagnostic);

                    context.RegisterCodeFix(
                        "Implement INotifyPropertyChanged fully qualified.",
                        (editor, cancellationToken) =>
                            ImplementINotifyPropertyChangedAsync(
                                editor,
                                addUsings: false,
                                cancellationToken),
                        "Implement INotifyPropertyChanged fully qualified.",
                        diagnostic);

                    async Task ImplementINotifyPropertyChangedAsync(DocumentEditor editor, bool addUsings, CancellationToken cancellationToken)
                    {
                        if (!type!.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, editor.SemanticModel.Compilation) &&
                            !HasINotifyPropertyChangedInterface(out var baseType))
                        {
                            if (baseType is null)
                            {
                                editor.AddInterfaceType(classDeclaration, INotifyPropertyChangedType);
                            }
                            else
                            {
                                _ = editor.ReplaceNode(
                                    baseType.Type,
                                    x => INotifyPropertyChangedType.WithTriviaFrom(x));
                            }
                        }

#pragma warning disable CS8602 // Dereference of a possibly null reference. CompilerBug
                        var nullabilityAnnotationsEnabled = editor.SemanticModel.GetNullableContext(classDeclaration.SpanStart).AnnotationsEnabled();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8604 // Possible null reference argument. CompilerBug
                        if (!type.TryFindEventRecursive("PropertyChanged", out _))
#pragma warning restore CS8604 // Possible null reference argument.
                        {
                            var eventDeclaration = (EventFieldDeclarationSyntax)editor.Generator.EventDeclaration(
                                "PropertyChanged",
                                InpcFactory.WithNullability(PropertyChangedEventHandlerType, nullabilityAnnotationsEnabled),
                                Accessibility.Public);

                            _ = editor.AddEvent(classDeclaration, eventDeclaration);
                        }

#pragma warning disable CS8604 // Possible null reference argument. CompilerBug
                        if (!type.TryFindFirstMethodRecursive("OnPropertyChanged", m => m.Parameters.TrySingle(out var parameter) && parameter.Type == KnownSymbol.String, out _))
#pragma warning restore CS8604 // Possible null reference argument.
                        {
                            await editor.AddOnPropertyChangedMethodAsync(
                                classDeclaration,
                                nullabilityAnnotationsEnabled,
                                cancellationToken).ConfigureAwait(false);
                        }

                        if (addUsings)
                        {
                            _ = editor.AddUsing(SyntaxFactory.UsingDirective(INotifyPropertyChangedType.Left))
                                      .AddUsing(SyntaxFactory.UsingDirective(CallerMemberNameType.Left));
                        }

                        bool HasINotifyPropertyChangedInterface(out BaseTypeSyntax? result)
                        {
#pragma warning disable CS8602 // Dereference of a possibly null reference. CompilerBug
                            if (classDeclaration.BaseList is { Types: { } types })
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            {
                                if (types.TryFirst<BaseTypeSyntax>(x => x.Type == KnownSymbol.INotifyPropertyChanged, out result))
                                {
                                    if (addUsings)
                                    {
                                        return true;
                                    }

                                    return result.Type.IsKind(SyntaxKind.QualifiedName);
                                }
                            }

                            result = null;
                            return false;
                        }
                    }
                }
            }

            bool References(string dll)
            {
                return semanticModel.Compilation.References.Any(x => x.Display?.EndsWith(dll, StringComparison.Ordinal) == true);
            }
        }

        private static bool IsSupportedDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic.Id == Descriptors.INPC001ImplementINotifyPropertyChanged.Id)
            {
                return true;
            }

            if (diagnostic.Id == "CS0535")
            {
                return diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                 .EndsWith("does not implement interface member 'INotifyPropertyChanged.PropertyChanged'", StringComparison.Ordinal);
            }

            return IsINotifyPropertyChangedMissing(diagnostic);
        }

        private static bool IsINotifyPropertyChangedMissing(Diagnostic diagnostic)
        {
            if (diagnostic.Id == "CS0246")
            {
                return diagnostic.GetMessage(CultureInfo.InvariantCulture) == "The type or namespace name 'INotifyPropertyChanged' could not be found (are you missing a using directive or an assembly reference?)";
            }

            return false;
        }
    }
}
