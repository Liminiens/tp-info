using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Operations;
using TPQuickInfo.Documentation;
using TPQuickInfo.Utility;

namespace TPQuickInfo.SignatureHelp
{
    internal class TypeProviderSignatureHelpSource : ISignatureHelpSource
    {
        private bool _isDisposed;
        private readonly ITextBuffer _textBuffer;
        private readonly TypeProviderSignatureHelpSourceProvider _provider;

        public TypeProviderSignatureHelpSource(
            ITextBuffer textBuffer,
            TypeProviderSignatureHelpSourceProvider provider)
        {
            _textBuffer = textBuffer;
            _provider = provider;
        }

        private ISignature CreateSignature(ITextBuffer textBuffer, string methodSig, string methodDoc, ITrackingSpan span)
        {
            TypeProviderSignature sig = new TypeProviderSignature(textBuffer, methodSig, methodDoc, null);
            textBuffer.Changed += sig.OnSubjectBufferChanged;

            string[] pars = methodSig.Split('(', ',', ')');
            List<IParameter> paramList = new List<IParameter>();

            int locusSearchStart = 0;
            for (int i = 1; i < pars.Length; i++)
            {
                string param = pars[i].Trim();

                if (string.IsNullOrEmpty(param))
                {
                    continue;
                }

                int locusStart = methodSig.IndexOf(param, locusSearchStart, StringComparison.Ordinal);
                if (locusStart >= 0)
                {
                    Span locus = new Span(locusStart, param.Length);
                    locusSearchStart = locusStart + param.Length;
                    paramList.Add(new TypeProviderParameter("Documentation for the parameter.", locus, param, sig));
                }
            }

            sig.Parameters = new ReadOnlyCollection<IParameter>(paramList);
            sig.ApplicableToSpan = span;
            sig.ComputeCurrentParameter();
            return sig;
        }

        /// <inheritdoc />
        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(_textBuffer);
            int position = triggerPoint.GetPosition(snapshot);

            ITrackingSpan applicableToSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(
                new Span(position, 0), SpanTrackingMode.EdgeInclusive, 0);

            signatures.Add(CreateSignature(_textBuffer, "add(int firstInt, int secondInt)", "Documentation for adding integers.", applicableToSpan));
            signatures.Add(CreateSignature(_textBuffer, "add(double firstDouble, double secondDouble)", "Documentation for adding doubles.", applicableToSpan));

            var document = _textBuffer.CurrentSnapshot.GetOpenDocumentInCurrentContextWithChanges();
            var semanticModel = document.GetSemanticModelAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var root = document.GetSyntaxRootAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            SnapshotPoint point = session.TextView.Caret.Position.BufferPosition - 2;
            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(point);
            var node = root.FindNode(TextSpan.FromBounds(extent.Span.Start, extent.Span.End));

            var symbolInfo = semanticModel.GetSymbolInfo(node);
            if (symbolInfo.IsProvidedType())
            {
                var attributeReader = new TypeProviderXmlDocAttributeReader(symbolInfo);
                var documentation = attributeReader.GetDocumentation();
            }
        }

        /// <inheritdoc />
        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            if (session.Signatures.Count > 0)
            {
                return session.SelectedSignature;
            }
            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }
}