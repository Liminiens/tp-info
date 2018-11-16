using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace TPQuickInfo.SignatureHelp
{
    internal class TypeProviderParameter : IParameter
    {
        public TypeProviderParameter(string documentation, Span locus, string name, ISignature signature)
        {
            Documentation = documentation;
            Locus = locus;
            Name = name;
            Signature = signature;
        }

        /// <inheritdoc />
        public ISignature Signature { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Documentation { get; }

        /// <inheritdoc />
        public Span Locus { get; }

        /// <inheritdoc />
        public Span PrettyPrintedLocus { get;  }
    }
}