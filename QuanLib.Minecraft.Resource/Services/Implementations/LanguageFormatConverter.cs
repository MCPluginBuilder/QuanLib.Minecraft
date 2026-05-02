using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public partial class LanguageFormatConverter : ILanguageFormatConverter
    {
        private const string ESC_PERCENT = "\u0001";
        private const string ESC_OPEN_BRACE = "\u0002";
        private const string ESC_CLOSE_BRACE = "\u0003";

        /// <summary>
        /// 将 Java 格式化字符串转换为 C# 格式化字符串。
        /// 支持 %s、%1$s、%% 等，并自动转义原始字符串中的花括号。
        /// </summary>
        /// <param name="languageFormat">Java 格式化字符串</param>
        /// <returns>C# 格式化字符串</returns>
        public string ConvertToCSharpFormat(string languageFormat)
        {
            ArgumentNullException.ThrowIfNull(languageFormat, nameof(languageFormat));

            languageFormat = languageFormat.Replace("%%", ESC_PERCENT);
            languageFormat = languageFormat.Replace("{", ESC_OPEN_BRACE);
            languageFormat = languageFormat.Replace("}", ESC_CLOSE_BRACE);

            Regex regex = MatchFormatRegex();
            int implicitIdx = 0;

            languageFormat = regex.Replace(languageFormat, match =>
            {
                int csharpIdx;
                if (match.Groups[1].Success)
                {
                    int javaIdx = int.Parse(match.Groups[1].Value);
                    csharpIdx = javaIdx - 1;
                }
                else
                {
                    csharpIdx = implicitIdx++;
                }
                return $"{{{csharpIdx}}}";
            });

            languageFormat = languageFormat.Replace(ESC_PERCENT, "%");
            languageFormat = languageFormat.Replace(ESC_OPEN_BRACE, "{{");
            languageFormat = languageFormat.Replace(ESC_CLOSE_BRACE, "}}");

            return languageFormat;
        }

        /// <summary>
        /// 将 C# 格式化字符串转换为匹配该格式的正则表达式模式。
        /// </summary>
        /// <param name="csharpFormat">C# 格式化字符串</param>
        /// <returns>匹配该格式的正则表达式模式</returns>
        public string ConvertToRegexPattern(string csharpFormat)
        {
            StringBuilder stringBuilder = new();
            int lastIndex = 0;

            foreach (Match match in MatchPatternRegex().Matches(csharpFormat))
            {
                // 添加 token 之前的文本，并通过 Regex.Escape 转义
                if (match.Index > lastIndex)
                    stringBuilder.Append(Regex.Escape(csharpFormat[lastIndex..match.Index]));

                // 按 token 类型追加对应的正则片段
                string token = match.Value;
                if (token == "{{")
                    stringBuilder.Append(@"\{");
                else if (token == "}}")
                    stringBuilder.Append(@"\}");
                else
                    stringBuilder.Append(@"([\s\S]+)"); // 占位符

                lastIndex = match.Index + match.Length;
            }

            // 处理末尾剩余文本
            if (lastIndex < csharpFormat.Length)
                stringBuilder.Append(Regex.Escape(csharpFormat[lastIndex..]));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 从 C# 格式化字符串中按出现顺序提取所有占位符的索引。
        /// 例如："将{2}的{1}增加了{0}（现在是{3}）" 返回 [2, 1, 0, 3]。
        /// </summary>
        /// <param name="csharpFormat">C# 格式化字符串</param>
        /// <returns>按出现顺序排列的占位符索引数组</returns>
        public int[] GetInterpolatedOrder(string csharpFormat)
        {
            ArgumentNullException.ThrowIfNull(csharpFormat, nameof(csharpFormat));

            if (csharpFormat == string.Empty)
                return Array.Empty<int>();

            // 匹配未被转义的 {index[,alignment][:formatString]}
            // (?<!\{) 保证 { 前不是 { ；(?!\}) 保证 } 后不是 } ，从而排除 {{ 和 }}
            MatchCollection matches = MatchOrderRegex().Matches(csharpFormat);

            return matches
                .Cast<Match>()
                .Select(m => int.Parse(m.Groups[1].Value))
                .ToArray();
        }

        [GeneratedRegex(@"%(?:(\d+)\$)?([a-zA-Z])")]
        private static partial Regex MatchFormatRegex();

        [GeneratedRegex(@"\{\{|\}\}|\{[^}]*\}")]
        private static partial Regex MatchPatternRegex();

        [GeneratedRegex(@"(?<!\{)\{(\d+)[^}]*\}(?!\})")]
        private static partial Regex MatchOrderRegex();
    }
}
