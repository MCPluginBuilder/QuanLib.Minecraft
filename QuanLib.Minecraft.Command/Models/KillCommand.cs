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
    public class KillCommand : MultipleCommandBase, ICreatible<KillCommand>
    {
        public KillCommand(LanguageManager languageManager)
        {
            ArgumentNullException.ThrowIfNull(languageManager, nameof(languageManager));

            MultipleOutput = languageManager["commands.kill.success.multiple"];
            Output = languageManager["commands.kill.success.single"];
            Input = LanguageTemplate.Parse("kill %s");
        }

        public override LanguageTemplate Input { get; }

        public override LanguageTemplate Output { get; }

        public override LanguageTemplate MultipleOutput { get; }

        public bool TrySendCommand(CommandSender sender, string target, out int result)
        {
            ArgumentException.ThrowIfNullOrEmpty(target, nameof(target));

            return base.TrySendCommand(sender, [target], out result);
        }

        protected override bool TryParseResult(string[] outargs, [MaybeNullWhen(false)] out int result)
        {
            return base.TryParseResult(outargs, 1, 0, out result);
        }

        public static KillCommand Create(LanguageManager languageManager)
        {
            return new KillCommand(languageManager);
        }
    }
}
