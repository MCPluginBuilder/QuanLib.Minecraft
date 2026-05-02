using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class LanguageTemplateParser : ILanguageTemplateParser
    {
        public LanguageTemplateParser(ILanguageFormatConverter formatConverter)
        {
            ArgumentNullException.ThrowIfNull(formatConverter, nameof(formatConverter));

            _formatConverter = formatConverter;
        }

        private readonly ILanguageFormatConverter _formatConverter;

        public Dictionary<string, LanguageTemplate> ParseLanguageTemplates(IReadOnlyDictionary<string, string> languageEntries)
        {
            ArgumentNullException.ThrowIfNull(languageEntries, nameof(languageEntries));

            Dictionary<string, LanguageTemplate> result = [];
            foreach (var entry in languageEntries)
            {
                string javaFormat = entry.Value;
                string csharpFormat = _formatConverter.ConvertToCSharpFormat(javaFormat);
                string regexPattern = _formatConverter.ConvertToRegexPattern(csharpFormat);
                int[] interpolatedOrder = _formatConverter.GetInterpolatedOrder(csharpFormat);

                LanguageTemplate languageTemplate = new(javaFormat, csharpFormat, regexPattern, interpolatedOrder);
                result.Add(entry.Key, languageTemplate);
            }

            return result;
        }
    }
}
