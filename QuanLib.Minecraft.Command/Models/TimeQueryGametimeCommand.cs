using QuanLib.Minecraft.Resource;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.Command.Models
{
    public class TimeQueryGametimeCommand : TimeQueryCommandBase, ICreatible<TimeQueryGametimeCommand>
    {
        public TimeQueryGametimeCommand(LanguageManager languageManager) : base(languageManager)
        {
            Input = LanguageTemplate.Parse("time query gametime");
        }

        public override LanguageTemplate Input { get; }

        public static TimeQueryGametimeCommand Create(LanguageManager languageManager)
        {
            return new TimeQueryGametimeCommand(languageManager);
        }
    }
}
