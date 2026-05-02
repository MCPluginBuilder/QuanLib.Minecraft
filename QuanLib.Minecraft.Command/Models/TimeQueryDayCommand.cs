using QuanLib.Minecraft.Command.Building;
using QuanLib.Minecraft.Resource;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.Command.Models
{
    public class TimeQueryDayCommand : TimeQueryCommandBase, ICreatible<TimeQueryDayCommand>
    {
        public TimeQueryDayCommand(LanguageManager languageManager) : base(languageManager)
        {
            Input = LanguageTemplate.Parse("time query day");
        }

        public override LanguageTemplate Input { get; }

        public static TimeQueryDayCommand Create(LanguageManager languageManager)
        {
            return new TimeQueryDayCommand(languageManager);
        }
    }
}
