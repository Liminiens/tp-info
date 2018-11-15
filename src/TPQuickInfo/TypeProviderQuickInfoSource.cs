using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace TPQuickInfo
{
    internal class TypeProviderQuickInfoSource : IAsyncQuickInfoSource
    {
        private bool _isDisposed;
        private readonly TypeProviderQuickInfoSourceProvider _provider;
        private readonly ITextBuffer _subjectBuffer;
        private readonly Dictionary<string, string> _descriptions;

        public TypeProviderQuickInfoSource(TypeProviderQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            _provider = provider;
            _subjectBuffer = subjectBuffer;

            //these are the method names and their descriptions
            _descriptions = new Dictionary<string, string>
            {
                {"add", "int add(int firstInt, int secondInt)\nAdds one integer to another."},
                {"subtract", "int subtract(int firstInt, int secondInt)\nSubtracts one integer from another."},
                {"multiply", "int multiply(int firstInt, int secondInt)\nMultiplies one integer by another."},
                {"divide", "int divide(int firstInt, int secondInt)\nDivides one integer by another."}
            };
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                return Task.FromResult<QuickInfoItem>(null);
            }

            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            //look for occurrences of our QuickInfo words in the span
            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_subjectBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            string searchText = extent.Span.GetText();

            foreach (string key in _descriptions.Keys)
            {
                int foundIndex = searchText.IndexOf(key, StringComparison.CurrentCultureIgnoreCase);
                if (foundIndex > -1)
                {
                    var applicableToSpan = currentSnapshot.CreateTrackingSpan
                    (
                        //querySpan.Start.Add(foundIndex).Position, 9, SpanTrackingMode.EdgeInclusive
                        extent.Span.Start + foundIndex, key.Length, SpanTrackingMode.EdgeInclusive
                    );
                    _descriptions.TryGetValue(key, out var value);
                    if (value != null)
                    {
                        return Task.FromResult(new QuickInfoItem(applicableToSpan, value));
                    }
                    else
                    {
                        return Task.FromResult(new QuickInfoItem(applicableToSpan, String.Empty));
                    }
                }
            }
            return Task.FromResult<QuickInfoItem>(null);
        }
    }
}
