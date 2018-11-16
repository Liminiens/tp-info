using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using TPQuickInfo.Utility;

namespace TPQuickInfo.QuickInfo
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("Type Provider QuickInfo Controller Provider")]
    [ContentType(PredefinedNames.RoslynLanguages)]
    internal class TypeProviderQuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        internal IAsyncQuickInfoBroker QuickInfoBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new TypeProviderQuickInfoController(textView, subjectBuffers, this);
        }
    }
}