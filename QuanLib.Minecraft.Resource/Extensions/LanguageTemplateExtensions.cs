using QuanLib.Minecraft.Resource.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace QuanLib.Minecraft.Resource.Extensions
{
    public static class LanguageTemplateExtensions
    {
        private static readonly LanguageFormatConverter _formatConverter = new();

        extension (LanguageTemplate template)
        {
            public int ParameterCount => template.InterpolatedOrder.Count;

            public static LanguageTemplate Parse(string javaFormat)
            {
                ArgumentNullException.ThrowIfNull(javaFormat);

                string csharpFormat = _formatConverter.ConvertToCSharpFormat(javaFormat);
                string regexPattern = _formatConverter.ConvertToRegexPattern(csharpFormat);
                int[] interpolatedOrder = _formatConverter.GetInterpolatedOrder(csharpFormat);

                return new LanguageTemplate(javaFormat, csharpFormat, regexPattern, interpolatedOrder);
            }

            public bool TryFormat(object[] args, [MaybeNullWhen(false)] out string output)
            {
                int parameterCount = template.InterpolatedOrder.Count;
                if (args is null || args.Length != parameterCount)
                    goto fail;

                if (parameterCount == 0)
                {
                    output = template.JavaFormat;
                    return true;
                }

                try
                {
                    output = string.Format(template.CSharpFormat, args);
                    return true;
                }
                catch (FormatException)
                {
                    goto fail;
                }

                fail:
                output = null;
                return false;
            }

            public bool TryMatch(string input, [MaybeNullWhen(false)] out string[] result)
            {
                if (string.IsNullOrEmpty(input))
                    goto fail;

                int parameterCount = template.InterpolatedOrder.Count;
                if (parameterCount == 0)
                {
                    if (input != template.JavaFormat)
                        goto fail;

                    result = Array.Empty<string>();
                    return true;
                }

                List<string> list = [];
                Match match = Regex.Match(input, template.RegexPattern);
                if (!match.Success)
                    goto fail;

                for (int i = 1; i < match.Groups.Count; i++)
                {
                    Group group = match.Groups[i];
                    list.Add(group.Value);
                }

                result = new string[list.Count];
                for (int i = 0; i < result.Length; i++)
                    result[i] = list[template.InterpolatedOrder[i]];
                return true;

                fail:
                result = null;
                return false;
            }
        }
    }
}
