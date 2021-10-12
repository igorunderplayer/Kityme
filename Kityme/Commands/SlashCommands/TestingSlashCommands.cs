using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Kityme.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kityme.Commands
{
    public class TestingSlashCommands : ApplicationCommandModule
    {
        [SlashCommand("ping", "mostra meu ping/latencia")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
            {
                Content = $"meu ping eh {ctx.Client.Ping}ms"
            });
        }
    }
}