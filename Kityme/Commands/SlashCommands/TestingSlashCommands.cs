﻿using DSharpPlus;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace Kityme.Commands
{
    public class TestingSlashCommands: ApplicationCommandModule
    {
        [SlashCommand("ping", "mostra meu ping/latencia")]
        public async Task Ping (InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {
                Content = $"meu ping eh {ctx.Client.Ping}ms"
            });
        }
    }
}
