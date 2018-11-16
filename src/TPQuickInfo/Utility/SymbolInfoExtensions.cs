﻿using Microsoft.CodeAnalysis;
using System.Linq;
using TPQuickInfo.Documentation;

namespace TPQuickInfo.Utility
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