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
    public class TelePortLocationCommand : MultipleCommandBase, ICreatible<TelePortLocationCommand>
    {
        public TelePortLocationCommand(LanguageManager languageManager)
        {
            ArgumentNullException.ThrowIfNull(languageManager, nameof(languageManager));

            MultipleOutput = languageManager["commands.teleport.success.location.multiple"];
            Output = languageManager["commands.teleport.success.location.single"];
            Input = LanguageTemplate.Parse("tp %s %s %s %s");
        }

        public override LanguageTemplate Input { get; }

        public override LanguageTemplate Output { get; }

        public override LanguageTemplate MultipleOutput { get; }

        public bool TrySendCommand(CommandSender sender, string source, double x, double y, double z, out int result)
        {
            ArgumentException.ThrowIfNullOrEmpty(source, nameof(source));

            return base.TrySendCommand(sender, [source, x, y, z], out result);
        }

        protected override bool TryParseResult(string[] outargs, [MaybeNullWhen(false)] out int result)
        {
            return base.TryParseResult(outargs, 4, 0, out result);
        }

        public static TelePortLocationCommand Create(LanguageManager languageManager)
        {
            return new TelePortLocationCommand(languageManager);
        }
    }
}
