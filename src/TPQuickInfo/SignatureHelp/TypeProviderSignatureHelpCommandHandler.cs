using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TPQuickInfo.Documentation;
using TPQuickInfo.Utility;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace TPQuickInfo.SignatureHelp
{
    internal sealed class TypeProviderSignatureHelpCommandHandler : IOleCommandTarget
    {
        private readonly IOleCommandTarget _nextCommandHandler;
        private readonly ITextView _textView;
        private readonly ISignatureHelpBroker _broker;
        private ISignatureHelpSession _session;
        private readonly ITextStructureNavigator _navigator;

        internal TypeProviderSignatureHelpCommandHandler(IVsTextView textViewAdapter, ITextView textView, ITextStructureNavigator nav, ISignatureHelpBroker broker)
        {
            _textView = textView;
            _broker = broker;
            _navigator = nav;
            //add this to the filter chain
            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        /// <inheritdoc />
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        /// <inheritdoc />
        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                var typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
                if (typedChar.Equals('('))
                {
                    var document = _textView.TextBuffer.CurrentSnapshot.GetOpenDocumentInCurrentContextWithChanges();

                    var semanticModel = document.GetSemanticModelAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    var root = document.GetSyntaxRootAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                    //move the point back so it's in the preceding word
                    SnapshotPoint point = _textView.Caret.Position.BufferPosition - 1;
                    TextExtent extent = _navigator.GetExtentOfWord(point);

                    var node = root.FindNode(TextSpan.FromBounds(extent.Span.Start, extent.Span.End));
                    var symbolInfo = semanticModel.GetSymbolInfo(node);
                    if (symbolInfo.IsProvidedType())
                    {
                        _session = _broker.TriggerSignatureHelp(_textView);
                    }
                }
                else if (typedChar.Equals(')') && _session != null)
                {
                    _session.Dismiss();
                    _session = null;
                }
            }
            return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
    }
}