using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace TPQuickInfo
{
    public class TypeProviderXmlDocAttributeReader
    {
        public const string AttributeName = "TypeProviderXmlDocAttribute";

        private static readonly Regex SummaryRegex = new Regex(@"<summary>(?<text>.*)</summary>", RegexOptions.Compiled);

        public string GetValue(in SymbolInfo symbolInfo)
        {
            var docAttribute =
                symbolInfo.Symbol
                    .GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass.Name == AttributeName);
            var documentation = docAttribute.ConstructorArguments.FirstOrDefault().Value.ToString();
            if (!String.IsNullOrEmpty(documentation))
            {
                var match = SummaryRegex.Match(documentation);
                if (match.Success)
                {
                    return match.Groups["text"].Value;
                }
                return documentation;
            }

            return String.Empty;
        }
    }
}
