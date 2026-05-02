using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Resource;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.Command.Models
{
    public class SetBlockCommand : CommandBase, ICreatible<SetBlockCommand>
    {
        public SetBlockCommand(LanguageManager languageManager)
        {
            ArgumentNullException.ThrowIfNull(languageManager, nameof(languageManager));

            Output = languageManager["commands.setblock.success"];
            Input = LanguageTemplate.Parse("setblock %s %s %s %s");
        }

        public override LanguageTemplate Input { get; }

        public override LanguageTemplate Output { get; }

        public bool TrySendCommand(CommandSender sender, int x, int y, int z, string blockId)
        {
            ArgumentNullException.ThrowIfNull(blockId, nameof(blockId));

            return base.TrySendCommand(sender, [x, y, z, blockId], out _);
        }

        public static SetBlockCommand Create(LanguageManager languageManager)
        {
            return new SetBlockCommand(languageManager);
        }
    }
}
