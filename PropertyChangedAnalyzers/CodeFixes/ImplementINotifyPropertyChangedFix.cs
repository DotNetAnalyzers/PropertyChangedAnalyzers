namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementINotifyPropertyChangedFix))]
    [Shared]
    internal class ImplementINotifyPropertyChangedFix : CodeFixProvider
    {
        // ReSharper disable once InconsistentNaming
        private static readonly QualifiedNameSyntax INotifyPropertyChangedType = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("System.ComponentModel.INotifyPropertyChanged")
                                                                                                                   .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                   .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax CallerMemberNameType = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("System.Runtime.CompilerServices.CallerMemberName")
                                                                                                             .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                             .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax PropertyChangedEventHandlerType = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("System.ComponentModel.PropertyChangedEventHandler")
                                                                                                                        .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                        .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax MvvmLightViewModelBaseType = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("GalaSoft.MvvmLight.ViewModelBase")
                                                                                                                   .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                   .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax CaliburnMicroPropertyChangedBase = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("Caliburn.Micro.PropertyChangedBase")
                                                                                                                         .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                         .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax StyletPropertyChangedBase = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("Stylet.PropertyChangedBase")
                                                                                                                  .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                  .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax MvvmCrossMvxNotifyPropertyChanged = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("MvvmCross.ViewModels.MvxNotifyPropertyChanged")
                                                                                                                              .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                              .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax MvvmCrossMvxViewModel = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("MvvmCross.ViewModels.MvxViewModel")
                                                                                                                  .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                  .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax MvvmCrossCoreMvxNotifyPropertyChanged = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged")
                                                                                                                 .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                                 .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax MvvmCrossCoreMvxViewModel = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("MvvmCross.Core.ViewModels.MvxViewModel")
                                                                                                     .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                     .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly QualifiedNameSyntax PrismMvvmBindableBase = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName("Microsoft.Practices.Prism.Mvvm.BindableBase")
                                                                                                              .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                                              .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC001ImplementINotifyPropertyChanged.Id,
            "CS0535",
            "CS0246");

        public override FixAllProvider GetFixAllProvider() => null;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (!IsSupportedDiagnostic(diagnostic))
                {
                    continue;
                }

                var classDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDeclaration == null)
                {
                    continue;
                }

                var type = (ITypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
                if (type.BaseType.TryFindEventRecursive("PropertyChanged", out _))
                {
                    continue;
                }

                if (type.BaseType == KnownSymbol.Object &&
                    !type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, semanticModel.Compilation))
                {
                    if (semanticModel.Compilation.References.Any(x => x.Display?.EndsWith("GalaSoft.MvvmLight.dll") == true))
                    {
                        RegisterSubclassFixes(diagnostic, classDeclaration, MvvmLightViewModelBaseType);
                    }

                    if (semanticModel.Compilation.References.Any(x => x.Display?.EndsWith("Caliburn.Micro.dll") == true))
                    {
                        RegisterSubclassFixes(diagnostic, classDeclaration, CaliburnMicroPropertyChangedBase);
                    }

                    if (semanticModel.Compilation.References.Any(x => x.Display?.EndsWith("Stylet.dll") == true))
                    {
                        RegisterSubclassFixes(diagnostic, classDeclaration, StyletPropertyChangedBase);
                    }

                    if (semanticModel.Compilation.References.Any(x => x.Display?.EndsWith("MvvmCross.dll") == true))
                    {
                        RegisterSubclassFixes(diagnostic, classDeclaration, MvvmCrossMvxNotifyPropertyChanged);
                        RegisterSubclassFixes(diagnostic, classDeclaration, MvvmCrossMvxViewModel);
                    }

                    if (semanticModel.Compilation.References.Any(x => x.Display?.EndsWith("MvvmCross.Core.dll") == true))
                    {
                        RegisterSubclassFixes(diagnostic, classDeclaration, MvvmCrossCoreMvxNotifyPropertyChanged);
                        RegisterSubclassFixes(diagnostic, classDeclaration, MvvmCrossCoreMvxViewModel);
                    }

                    if (semanticModel.Compilation.References.Any(x => x.Display?.EndsWith("Prism.Mvvm.dll") == true))
                    {
                        RegisterSubclassFixes(diagnostic, classDeclaration, PrismMvvmBindableBase);
                    }
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Implement INotifyPropertyChanged and add usings.",
                        cancellationToken =>
                            ImplementINotifyPropertyChangedAsync(
                                context,
                                semanticModel,
                                classDeclaration,
                                cancellationToken),
                        this.GetType().FullName),
                    diagnostic);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Implement INotifyPropertyChanged fully qualified.",
                        cancellationToken =>
                            ImplementINotifyPropertyChangedFullyQualifiedAsync(
                                context,
                                semanticModel,
                                classDeclaration,
                                cancellationToken),
                        this.GetType().FullName),
                    diagnostic);
            }

            void RegisterSubclassFixes(Diagnostic diagnostic, ClassDeclarationSyntax classDeclaration, QualifiedNameSyntax viewModelBasetype)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        $"Subclass {viewModelBasetype} and add using.",
                        cancellationToken =>
                            SubclassViewModelBaseAndAddUsingAsync(
                                context,
                                classDeclaration,
                                viewModelBasetype,
                                cancellationToken),
                        $"Subclass {viewModelBasetype} add usings."),
                    diagnostic);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        $"Subclass {viewModelBasetype} fully qualified.",
                        cancellationToken =>
                            SubclassViewModelBaseFullyQualifiedAsync(
                                context,
                                classDeclaration,
                                viewModelBasetype,
                                cancellationToken),
                        $"Subclass {viewModelBasetype} fully qualified."),
                    diagnostic);
            }
        }

        private static async Task<Document> ImplementINotifyPropertyChangedAsync(CodeFixContext context, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
                                             .ConfigureAwait(false);
            ImplementINotifyPropertyChanged(context, semanticModel, classDeclaration, editor);
            _ = editor.AddUsing(SyntaxFactory.UsingDirective(INotifyPropertyChangedType.Left))
                      .AddUsing(SyntaxFactory.UsingDirective(CallerMemberNameType.Left));
            return editor.GetChangedDocument();
        }

        private static async Task<Document> ImplementINotifyPropertyChangedFullyQualifiedAsync(CodeFixContext context, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
                                             .ConfigureAwait(false);
            ImplementINotifyPropertyChanged(context, semanticModel, classDeclaration, editor);
            return editor.GetChangedDocument();
        }

        private static void ImplementINotifyPropertyChanged(CodeFixContext context, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, DocumentEditor editor)
        {
            var type = (ITypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
            var underscoreFields = semanticModel.UnderscoreFields() == CodeStyleResult.Yes;
            if (!type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, semanticModel.Compilation))
            {
                if (classDeclaration.BaseList != null &&
                    classDeclaration.BaseList.Types.TryFirst(
                        x => (x.Type as IdentifierNameSyntax)?.Identifier.ValueText.Contains("INotifyPropertyChanged") == true,
                        out var baseType) &&
                    context.Diagnostics.Any(d => IsINotifyPropertyChangedMissing(d)))
                {
                    editor.ReplaceNode(baseType, SyntaxFactory.SimpleBaseType(INotifyPropertyChangedType));
                }
                else
                {
                    editor.AddInterfaceType(classDeclaration, INotifyPropertyChangedType);
                }
            }

            if (!type.TryFindEventRecursive("PropertyChanged", out _))
            {
                _ = editor.AddEvent(
                    classDeclaration,
                    (EventFieldDeclarationSyntax)editor.Generator.EventDeclaration("PropertyChanged", PropertyChangedEventHandlerType, Accessibility.Public));
            }

            if (!type.TryFindFirstMethodRecursive(
                "OnPropertyChanged",
                m => m.Parameters.Length == 1 &&
                     m.Parameters[0]
                      .Type == KnownSymbol.String,
                out _))
            {
                if (type.IsSealed)
                {
                    _ = editor.AddMethod(
                        classDeclaration,
                        ParseMethod(
                            @"private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
                              {
                                  this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                              }",
                            underscoreFields));
                }
                else
                {
                    _ = editor.AddMethod(
                        classDeclaration,
                        ParseMethod(
                            @"protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
                              {
                                  this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                              }",
                            underscoreFields));
                }
            }
        }

        private static async Task<Document> SubclassViewModelBaseAndAddUsingAsync(CodeFixContext context, ClassDeclarationSyntax classDeclaration, QualifiedNameSyntax viewModelBaseType, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
                                             .ConfigureAwait(false);
            AddBaseType(context, classDeclaration, viewModelBaseType, editor);
            _ = editor.AddUsing(SyntaxFactory.UsingDirective(viewModelBaseType.Left));
            return editor.GetChangedDocument();
        }

        private static async Task<Document> SubclassViewModelBaseFullyQualifiedAsync(CodeFixContext context, ClassDeclarationSyntax classDeclaration, QualifiedNameSyntax viewModelBaseType, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
                                             .ConfigureAwait(false);
            AddBaseType(context, classDeclaration, viewModelBaseType, editor);
            return editor.GetChangedDocument();
        }

        private static void AddBaseType(CodeFixContext context, ClassDeclarationSyntax classDeclaration, TypeSyntax viewModelBaseType, DocumentEditor editor)
        {
            if (classDeclaration.BaseList != null &&
                classDeclaration.BaseList.Types.TryFirst(
                    x => (x.Type as IdentifierNameSyntax)?.Identifier.ValueText.Contains("INotifyPropertyChanged") == true,
                    out var baseType) &&
                context.Diagnostics.Any(x => IsINotifyPropertyChangedMissing(x)))
            {
                editor.ReplaceNode(baseType, SyntaxFactory.SimpleBaseType(viewModelBaseType));
            }
            else
            {
                editor.AddBaseType(classDeclaration, viewModelBaseType);
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
                                 .EndsWith("does not implement interface member 'INotifyPropertyChanged.PropertyChanged'");
            }

            return IsINotifyPropertyChangedMissing(diagnostic);
        }

        private static bool IsINotifyPropertyChangedMissing(Diagnostic diagnostic)
        {
            if (diagnostic.Id == "CS0246")
            {
                return diagnostic.GetMessage(CultureInfo.InvariantCulture) ==
                       "The type or namespace name 'INotifyPropertyChanged' could not be found (are you missing a using directive or an assembly reference?)";
            }

            return false;
        }

        private static MethodDeclarationSyntax ParseMethod(string code, bool usesUnderscoreNames)
        {
            if (usesUnderscoreNames)
            {
                code = code.Replace("this.", string.Empty);
            }

            return Parse.MethodDeclaration(code)
                        .WithSimplifiedNames()
                        .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                        .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                        .WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}
