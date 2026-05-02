using QuanLib.Minecraft.Resource;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.Command.Models
{
    public class TimeQueryDaytimeCommand : TimeQueryCommandBase, ICreatible<TimeQueryDaytimeCommand>
    {
        public TimeQueryDaytimeCommand(LanguageManager languageManager) : base(languageManager)
        {
            Input = LanguageTemplate.Parse("time query daytime");
        }

        public override LanguageTemplate Input { get; }

        public static TimeQueryDaytimeCommand Create(LanguageManager languageManager)
        {
            return new TimeQueryDaytimeCommand(languageManager);
        }
    }
}
