using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Entities;
using System.Collections.Generic;
using System.Linq;
using Kityme.Attributes;
using System.Text;
using System;

namespace Kityme
{
    public class KitymeHelpFormatter : DefaultHelpFormatter
    {
        private DiscordEmbedBuilder embedBuilder;

        public KitymeHelpFormatter (CommandContext ctx) : base(ctx)
        {
            this.embedBuilder = new DiscordEmbedBuilder();

            var commands = ctx.Client.GetCommandsNext().RegisteredCommands;


            StringBuilder stringBuiler = new StringBuilder();
            ctx.RespondAsync(commands.Values.Where(cmd => cmd.Name == "botinfo").Count().ToString());
            var info = commands.Values.Where(cmd => cmd.Module.ModuleType.Name == "InfoCommands");

            stringBuiler.AppendLine($"**Info**").AppendLine();
            foreach (var infoCommand in info)
            {
                stringBuiler.Append($"`{infoCommand.Name}`, ");
            }

            embedBuilder.WithDescription(stringBuiler.ToString());
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: embedBuilder.Build());
        }
    }
}
