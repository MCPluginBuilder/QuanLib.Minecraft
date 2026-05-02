using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface ILanguageTemplateParser
    {
        public Dictionary<string, LanguageTemplate> ParseLanguageTemplates(IReadOnlyDictionary<string, string> languageEntries);
    }
}
