using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TPQuickInfo.Utility;

namespace TPQuickInfo.Documentation
{
    internal class TypeProviderXmlDocAttributeReader
    {
        private readonly SymbolInfo _symbolInfo;
        private DocumentationComment _documentationComment;

        private static readonly Regex SummaryRegex = new Regex(@"<summary>(?<text>.*)</summary>", RegexOptions.Compiled);
        public const string AttributeName = "TypeProviderXmlDocAttribute";

        public TypeProviderXmlDocAttributeReader(SymbolInfo symbolInfo)
        {
            _symbolInfo = symbolInfo;
        }

        public DocumentationComment GetDocumentation()
        {
            if (_documentationComment == null && _symbolInfo.Symbol != null)
            {
                var docAttribute =
                    _symbolInfo.Symbol
                        .GetAttributes()
                        .FirstOrDefault(x => x.AttributeClass.Name == AttributeName);
                if (docAttribute != null)
                {
                    var documentation = docAttribute.ConstructorArguments.FirstOrDefault().Value.ToString();
                    if (!String.IsNullOrEmpty(documentation))
                    {
                        var match = SummaryRegex.Match(documentation);
                        if (match.Success)
                        {
                            _documentationComment = DocumentationComment.FromXmlFragment(documentation);
                        }
                        else
                        {
                            _documentationComment = new DocumentationComment() { SummaryText = documentation };
                        }
                    }
                }
            }
            return _documentationComment;
        }
    }
}
