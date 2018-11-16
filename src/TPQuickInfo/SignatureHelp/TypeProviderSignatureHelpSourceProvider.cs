using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using TPQuickInfo.Utility;

namespace TPQuickInfo.SignatureHelp
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [Name("Type Provider Signature Help source")]
    [Order(Before = "default")]
    [ContentType(PredefinedNames.RoslynLanguages)]
    internal class TypeProviderSignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        /// <inheritdoc />
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return new TypeProviderSignatureHelpSource(textBuffer, this);
        }
    }
}