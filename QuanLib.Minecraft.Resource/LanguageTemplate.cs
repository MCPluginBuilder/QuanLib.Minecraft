using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public record class LanguageTemplate(string JavaFormat, string CSharpFormat, string RegexPattern, IReadOnlyList<int> InterpolatedOrder);
}
