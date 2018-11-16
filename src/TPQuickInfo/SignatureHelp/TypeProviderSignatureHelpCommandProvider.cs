using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using TPQuickInfo.Utility;

namespace TPQuickInfo.SignatureHelp
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Type Provider Signature Help controller")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [ContentType(PredefinedNames.RoslynLanguages)]
    internal class TypeProviderSignatureHelpCommandProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ISignatureHelpBroker SignatureHelpBroker { get; set; }

        /// <inheritdoc />
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);

            textView?.Properties.GetOrCreateSingletonProperty(
                () => new TypeProviderSignatureHelpCommandHandler(textViewAdapter,
                    textView,
                    NavigatorService.GetTextStructureNavigator(textView.TextBuffer),
                    SignatureHelpBroker));
        }
    }
}