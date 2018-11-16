using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace TPQuickInfo.QuickInfo
{
    internal class TypeProviderQuickInfoController : IIntellisenseController
    {
        private ITextView _textView;
        private readonly IList<ITextBuffer> _subjectBuffers;
        private readonly TypeProviderQuickInfoControllerProvider _provider;
        private IAsyncQuickInfoSession _session;

        internal TypeProviderQuickInfoController(
            ITextView textView,
            IList<ITextBuffer> subjectBuffers,
            TypeProviderQuickInfoControllerProvider provider)
        {
            _textView = textView;
            _subjectBuffers = subjectBuffers;
            _provider = provider;

            _textView.MouseHover += this.OnTextViewMouseHover;
        }

        private async void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //find the mouse position by mapping down to the subject buffer
            SnapshotPoint? point =
                _textView.BufferGraph.MapDownToFirstMatch(
                    new SnapshotPoint(_textView.TextSnapshot, e.Position),
                    PointTrackingMode.Positive,
                    snapshot => _subjectBuffers.Contains(snapshot.TextBuffer),
                    PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                    PointTrackingMode.Positive);

                if (!_provider.QuickInfoBroker.IsQuickInfoActive(_textView))
                {
                    _session = await _provider.QuickInfoBroker.TriggerQuickInfoAsync(_textView, triggerPoint);
                }
            }
        }

        public void Detach(ITextView textView)
        {
            if (_textView == textView)
            {
                _textView.MouseHover -= OnTextViewMouseHover;
                _textView = null;
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
