using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Entities;
using System.Collections.Generic;
using System.Linq;
using Kityme.Attributes;
using System.Text;

namespace Kityme
{
    public class KitymeHelpFormatter : BaseHelpFormatter
    {
        private DiscordEmbedBuilder embedBuilder;

        public KitymeHelpFormatter (CommandContext ctx) : base(ctx)
        {
            this.embedBuilder = new DiscordEmbedBuilder();

            var commands = ctx.Client.GetCommandsNext().RegisteredCommands;


            StringBuilder stringBuiler = new StringBuilder();

            var info = commands.Where(x => (x.Value.CustomAttributes.Where(x => x.GetType() == typeof(CommandTypeAttribute)).FirstOrDefault() as CommandTypeAttribute)?.Type == "Info").ToArray();
            ctx.RespondAsync(info.Count().ToString());

            stringBuiler.AppendLine($"**Info**").AppendLine();
            foreach (var infoCommand in info)
            {
                stringBuiler.Append($"`{infoCommand.Value.Name}`, ");
            }

            embedBuilder.WithDescription(stringBuiler.ToString());
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: embedBuilder.Build());
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            embedBuilder
                .WithTitle("Ajuda")
                .WithDescription($"`{command.Name}`: {command.Description}")
                .AddField("Aliases", string.Join(", ", command.Aliases));


            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            return this;
        }
    }
}
