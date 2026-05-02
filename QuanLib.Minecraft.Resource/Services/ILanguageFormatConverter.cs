using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface ILanguageFormatConverter
    {
        public string ConvertToCSharpFormat(string languageFormat);

        public string ConvertToRegexPattern(string csharpFormat);

        public int[] GetInterpolatedOrder(string csharpFormat);
    }
}
