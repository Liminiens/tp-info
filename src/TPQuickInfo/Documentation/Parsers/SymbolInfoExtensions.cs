using System.Linq;
using Microsoft.CodeAnalysis;

namespace TPQuickInfo.Documentation.Parsers
{
    public static class SymbolInfoExtensions
    {
        public static bool IsProvidedType(this SymbolInfo symbolInfo)
        {
            if (symbolInfo.Symbol != null)
            {
                return symbolInfo.Symbol
                    .GetAttributes()
                    .Any(x => x.AttributeClass.Name == TypeProviderXmlDocAttributeReader.AttributeName);
            }

            return false;
        }
    }
}