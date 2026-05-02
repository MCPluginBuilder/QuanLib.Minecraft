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
    public class SummonCommand : SummonCommandBase, ICreatible<SummonCommand>
    {
        public SummonCommand(LanguageManager languageManager) : base(languageManager)
        {
            Input = LanguageTemplate.Parse("summon %s %s %s %s");
        }

        public override LanguageTemplate Input { get; }

        public bool TrySendCommand(CommandSender sender, double x, double y, double z, string entityId)
        {
            ArgumentException.ThrowIfNullOrEmpty(entityId, nameof(entityId));

            return base.TrySendCommand(sender, [entityId, x, y, z], out _);
        }

        public static SummonCommand Create(LanguageManager languageManager)
        {
            return new SummonCommand(languageManager);
        }
    }
}
