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
    public class TellrawCommand : CommandBase, ICreatible<TellrawCommand>
    {
        public TellrawCommand()
        {
            Input = LanguageTemplate.Parse("tellraw %s %s");
            Output = LanguageTemplate.Parse("%s");
        }

        public override LanguageTemplate Input { get; }

        public override LanguageTemplate Output { get; }

        public bool TrySendCommand(CommandSender sender, string target, string message)
        {
            ArgumentException.ThrowIfNullOrEmpty(target, nameof(target));
            ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));

            return base.TrySendCommand(sender, [target, message], out _);
        }

        public static TellrawCommand Create(LanguageManager languageManager)
        {
            throw new NotImplementedException();
        }
    }
}
