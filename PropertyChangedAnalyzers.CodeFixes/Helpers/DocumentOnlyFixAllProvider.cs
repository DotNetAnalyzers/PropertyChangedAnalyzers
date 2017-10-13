namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;

    internal class DocumentOnlyFixAllProvider : FixAllProvider
    {
        public static readonly DocumentOnlyFixAllProvider Default = new DocumentOnlyFixAllProvider();

        private static readonly ImmutableArray<FixAllScope> SupportedFixAllScopes = ImmutableArray.Create(FixAllScope.Document);

        private DocumentOnlyFixAllProvider()
        {
        }

        public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
        {
            return SupportedFixAllScopes;
        }

        public override Task<CodeAction> GetFixAsync(FixAllContext context)
        {
            return WellKnownFixAllProviders.BatchFixer.GetFixAsync(
                new FixAllContext(
                    context.Document,
                    context.CodeFixProvider,
                    context.Scope,
                    context.CodeActionEquivalenceKey,
                    context.DiagnosticIds,
                    new DocumentOrderDiagnosticProvider(context),
                    context.CancellationToken));
        }

        private class DocumentOrderDiagnosticProvider : FixAllContext.DiagnosticProvider
        {
            private readonly FixAllContext context;

            public DocumentOrderDiagnosticProvider(FixAllContext context)
            {
                this.context = context;
            }

            public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
            {
                var diagnostics = await this.context.GetDocumentDiagnosticsAsync(document)
                                                    .ConfigureAwait(false);
                return diagnostics.OrderBy(x => x.Location.SourceSpan.Start);
            }

            public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
            {
                throw new System.NotSupportedException();
            }

            public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
            {
                throw new System.NotSupportedException();
            }
        }
    }
}