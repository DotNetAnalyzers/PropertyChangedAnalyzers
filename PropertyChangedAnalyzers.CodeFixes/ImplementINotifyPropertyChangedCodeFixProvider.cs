namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementINotifyPropertyChangedCodeFixProvider))]
    [Shared]
    internal class ImplementINotifyPropertyChangedCodeFixProvider : CodeFixProvider
    {
        // ReSharper disable once InconsistentNaming
        private static readonly TypeSyntax INotifyPropertyChangedType = SyntaxFactory.ParseTypeName("System.ComponentModel.INotifyPropertyChanged")
                                                                                     .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                     .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly TypeSyntax PropertyChangedEventHandlerType = SyntaxFactory.ParseTypeName("System.ComponentModel.PropertyChangedEventHandler")
                                                                                          .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                          .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        private static readonly TypeSyntax MvvmLightViewModelBaseType = SyntaxFactory.ParseTypeName("GalaSoft.MvvmLight.ViewModelBase")
                                                                                     .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                                                     .WithAdditionalAnnotations(Simplifier.Annotation, SyntaxAnnotation.ElasticAnnotation);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            INPC001ImplementINotifyPropertyChanged.DiagnosticId,
            "CS0535",
            "CS0246");

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

                if (semanticModel.Compilation.References.Any(x => x.Display?.EndsWith("GalaSoft.MvvmLight.dll") == true))
                {
                    var type = (ITypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
                    if (type.BaseType == KnownSymbol.Object &&
                        !type.Is(KnownSymbol.INotifyPropertyChanged))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Subclass GalaSoft.MvvmLight.ViewModelBase",
                                cancellationToken =>
                                    SubclassMvvmLightViewModelBaseAsync(
                                        context,
                                        classDeclaration,
                                        cancellationToken),
                                this.GetType().FullName + "Subclass GalaSoft.MvvmLight.ViewModelBase"),
                            diagnostic);
                    }
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Implement INotifyPropertyChanged.",
                        cancellationToken =>
                            ApplyImplementINotifyPropertyChangedFixAsync(
                                context,
                                semanticModel,
                                classDeclaration,
                                cancellationToken),
                        this.GetType().FullName),
                    diagnostic);
            }
        }

        private static async Task<Document> ApplyImplementINotifyPropertyChangedFixAsync(CodeFixContext context, SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var type = (ITypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
                                             .ConfigureAwait(false);
            var usesUnderscoreNames = classDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
            if (!type.Is(KnownSymbol.INotifyPropertyChanged))
            {
                if (classDeclaration.BaseList != null &&
                    classDeclaration.BaseList.Types.TryGetFirst(x => (x.Type as IdentifierNameSyntax)?.Identifier.ValueText.Contains("INotifyPropertyChanged") == true, out BaseTypeSyntax baseType) &&
                    context.Diagnostics.Any(IsINotifyPropertyChangedMissing))
                {
                    editor.ReplaceNode(baseType, SyntaxFactory.SimpleBaseType(INotifyPropertyChangedType));
                }
                else
                {
                    editor.AddInterfaceType(classDeclaration, INotifyPropertyChangedType);
                }
            }

            if (!type.TryGetEvent("PropertyChanged", out _))
            {
                editor.AddEvent(
                    classDeclaration,
                    (EventFieldDeclarationSyntax)editor.Generator.EventDeclaration(
                        "PropertyChanged",
                        PropertyChangedEventHandlerType,
                        Accessibility.Public));
            }

            if (!PropertyChanged.TryGetInvoker(type, semanticModel, cancellationToken, out _))
            {
                if (type.IsSealed)
                {
                    editor.AddMethod(
                        classDeclaration,
                        ParseMethod(
                            @"private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
                              {
                                  this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                              }",
                            usesUnderscoreNames));
                }
                else
                {
                    editor.AddMethod(
                        classDeclaration,
                        ParseMethod(
                            @"protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
                              {
                                  this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                              }",
                            usesUnderscoreNames));
                }

            }

            return editor.GetChangedDocument();
        }

        private static async Task<Document> SubclassMvvmLightViewModelBaseAsync(CodeFixContext context, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken)
                                             .ConfigureAwait(false);
            if (classDeclaration.BaseList != null &&
                classDeclaration.BaseList.Types.TryGetFirst(x => (x.Type as IdentifierNameSyntax)?.Identifier.ValueText.Contains("INotifyPropertyChanged") == true, out BaseTypeSyntax baseType) &&
                context.Diagnostics.Any(IsINotifyPropertyChangedMissing))
            {
                editor.ReplaceNode(baseType, SyntaxFactory.SimpleBaseType(MvvmLightViewModelBaseType));
            }
            else
            {
                editor.AddBaseType(classDeclaration, MvvmLightViewModelBaseType);
            }

            return editor.GetChangedDocument();
        }

        private static bool IsSupportedDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic.Id == INPC001ImplementINotifyPropertyChanged.DiagnosticId)
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

            return (MethodDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code)
                                                         .Members
                                                         .Single()
                                                         .WithSimplifiedNames()
                                                         .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                                                         .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                         .WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}