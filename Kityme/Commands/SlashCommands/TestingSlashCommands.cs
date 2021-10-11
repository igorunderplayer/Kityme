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

        [SlashCommand("search", "procura uma musca")]
        public async Task Search(InteractionContext ctx, [Option("Nome", "noem da musica q vc quer busca")] string query)
        {
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;
            DiscordChannel channel = voiceState?.Channel;

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "vc precisa ta num canal de voz" });
                return;
            }

            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "Lavalink n ta conectado" });
                return;
            }

            GuildMusicManager manager = null;
            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
                manager = MusicManagers._managers[ctx.Guild.Id];


            var node = manager?.Node ?? lava.ConnectedNodes.Values.First();

            var result = await node.Rest.GetTracksAsync(query);
            var tracks = result.Tracks;

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < tracks.Count(); i++)
            {
                LavalinkTrack track = tracks.ElementAt(i);
                stringBuilder.AppendLine($"{i + 1}| {track.Title}");
            }


            var response = new DiscordInteractionResponseBuilder()
                .WithContent($"resultado da busca: \n\n{stringBuilder}");

            if (manager != null)
            {
                DiscordSelectComponentOption[] options = new DiscordSelectComponentOption[tracks.Count()];
                for (int i = 0; i < tracks.Count(); i++)
                {
                    LavalinkTrack track = tracks.ElementAt(i);
                    // string json = JsonConvert.SerializeObject(track);
                    options[i] = new DiscordSelectComponentOption($"{i + 1}| {track.Title}", track.Uri.AbsoluteUri);
                }
                DiscordSelectComponent component = new DiscordSelectComponent("sm_select", "Selecione uma musica para adicionar", options, maxOptions: tracks.Count());
                response.AddComponents(component);
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }
    }
}