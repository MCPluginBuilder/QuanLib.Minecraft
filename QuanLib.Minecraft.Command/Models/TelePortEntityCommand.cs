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
    public class TelePortEntityCommand : MultipleCommandBase, ICreatible<TelePortEntityCommand>
    {
        public TelePortEntityCommand(LanguageManager languageManager)
        {
            ArgumentNullException.ThrowIfNull(languageManager, nameof(languageManager));

            MultipleOutput = languageManager["commands.teleport.success.entity.multiple"];
            Output = languageManager["commands.teleport.success.entity.single"];
            Input = LanguageTemplate.Parse("%s tp %s");
        }

        public override LanguageTemplate Input { get; }

        public override LanguageTemplate Output { get; }

        public override LanguageTemplate MultipleOutput { get; }

        public bool TrySendCommand(CommandSender sender, string source, string target, out int result)
        {
            ArgumentException.ThrowIfNullOrEmpty(source, nameof(source));
            ArgumentException.ThrowIfNullOrEmpty(target, nameof(target));

            return base.TrySendCommand(sender, [source, target], out result);
        }

        protected override bool TryParseResult(string[] outargs, [MaybeNullWhen(false)] out int result)
        {
            return base.TryParseResult(outargs, 2, 0, out result);
        }

        public static TelePortEntityCommand Create(LanguageManager languageManager)
        {
            return new TelePortEntityCommand(languageManager);
        }
    }
}
