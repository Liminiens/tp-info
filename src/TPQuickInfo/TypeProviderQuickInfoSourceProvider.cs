using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace TPQuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("Type Provider QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType("Roslyn Languages")]
    internal class TypeProviderQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new TypeProviderQuickInfoSource(this, textBuffer);
        }
    }
}
