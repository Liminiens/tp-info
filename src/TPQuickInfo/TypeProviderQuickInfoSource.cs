using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TPQuickInfo
{
    internal class TypeProviderQuickInfoSource : IAsyncQuickInfoSource
    {
        private bool _isDisposed;
        private readonly TypeProviderQuickInfoSourceProvider _provider;
        private readonly ITextBuffer _textBuffer;

        public TypeProviderQuickInfoSource(TypeProviderQuickInfoSourceProvider provider, ITextBuffer textBuffer)
        {
            _provider = provider;
            _textBuffer = textBuffer;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                return null;
            }

            var document = _textBuffer.CurrentSnapshot.GetOpenDocumentInCurrentContextWithChanges();
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);

            var node = root.FindNode(TextSpan.FromBounds(extent.Span.Start, extent.Span.End));
            var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
            var docAttribute =
                symbolInfo.Symbol
                    .GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass.Name == "TypeProviderXmlDocAttribute");
            if (docAttribute != null)
            {
                var summaryRegex = new Regex(@"<summary>(?<text>.*)</summary>", RegexOptions.Compiled);
                var documentation = docAttribute.ConstructorArguments.FirstOrDefault().Value.ToString();
                if (!String.IsNullOrEmpty(documentation))
                {
                    var match = summaryRegex.Match(documentation);
                    if (match.Success)
                    {
                        documentation = match.Groups["text"].Value;
                        if (!String.IsNullOrEmpty(documentation))
                        {
                            return CreateSpan();
                        }
                    }
                    return CreateSpan();
                    QuickInfoItem CreateSpan()
                    {
                        var applicableToSpan = currentSnapshot.CreateTrackingSpan
                        (
                            node.Span.Start, 1, SpanTrackingMode.EdgeInclusive
                        );
                        return new QuickInfoItem(applicableToSpan, documentation);
                    };
                }
            }
            return null;
        }
    }
}
