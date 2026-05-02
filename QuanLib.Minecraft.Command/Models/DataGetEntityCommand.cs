using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Resource;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.Command.Models
{
    public class DataGetEntityCommand : DataGetEntityCommandBase, ICreatible<DataGetEntityCommand>
    {
        public DataGetEntityCommand(LanguageManager languageManager) : base(languageManager)
        {
            Input = LanguageTemplate.Parse("data get entity %s");
        }

        public bool TrySendCommand(CommandSender sender, string target, [MaybeNullWhen(false)] out string result)
        {
            return base.TrySendCommand(sender, [target], out result);
        }

        public override LanguageTemplate Input { get; }

        public static DataGetEntityCommand Create(LanguageManager languageManager)
        {
            return new DataGetEntityCommand(languageManager);
        }
    }
}
