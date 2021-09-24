using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Kityme.Managers;

namespace Kityme.Commands.SlashCommands
{
    public class MusicSlashCommands: ApplicationCommandModule
    {
        [SlashCommand("play", "toca musica da sua escolha")]
        public async Task Play(InteractionContext ctx, [Option("musica", "musica q sera buscada")] string query)
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

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "anal invalido" });
                return;
            }

            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                if (ctx.Member.VoiceState.Channel.Id != MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "ja to num canal de voz" });
                    return;
                }
            }
            else
            {
                MusicManagers._managers.Add(ctx.Guild.Id, new GuildMusicManager(ctx.Client, ctx.Guild, channel, (id) => MusicManagers._managers.Remove(id)));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = $"Entrei no canal {channel.Name}!" });
            }
            Uri uri = null;
            if (Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute))
                uri = new Uri(query);

            PlayResponse res = await MusicManagers._managers[ctx.Guild.Id].Play(ctx.Channel, ctx.Member, query, uri);
            if (res.Type == PlayResponseType.SingleTrackLoad)
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = $"{res.Track.Title} adicionada na fila'-" });
            else if (res.Type == PlayResponseType.PlaylistLoad)
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "playlist carregada -" });
            else if(res.Type == PlayResponseType.TrackNotFound)
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "n achei" });
        }
        
        [SlashCommand("skip", "Pula a musica atual")]
        public async Task Skip (InteractionContext ctx, [Option("index", "numero da musica para ser pulada")] long skipTo = 0) 
        {
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;
            
                        if(skipTo < 0)
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "o valor n pode ser negativo" });
                            return;
                        }
            
                        if (voiceState == null || voiceState.Channel == null)
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "vc precisa estar em um canal de voz" });
                            return;
                        }
            
            
                        if(!MusicManagers._managers.ContainsKey(ctx.Guild.Id))
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "eu n to conectado aq" });
                            return;
                        }
            
                        if(MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id != voiceState.Channel.Id)
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "vc precisa ta no msm canal q eu" });
                            return;
                        }
            
                        if (!MusicManagers._managers[ctx.Guild.Id].CanChangeQueue(ctx.Member))
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "vc n pode mexer agr'-" });
                            return;
                        }
            
                        bool succes = await MusicManagers._managers[ctx.Guild.Id].Skip(skipTo == 0 ? -1 : (int)skipTo - 1);
                        if(!succes)
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "n foi possivel pular, certifique-se de colocar o numero certo ou se tem outra musica na fila" });
                        } else
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "🤸‍♂️pulado!" });
                        }
        }

        [SlashCommand("nowplaying", "mostra oq ta tocando")]
        public async Task NowPlaying(InteractionContext ctx)
        {
            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                GuildMusicManager manager = MusicManagers._managers[ctx.Guild.Id];
                LavalinkTrack npTrack = manager._queue[manager.ActualIndex];

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Now Playing",
                    Description = $"Atualmente estou tocando `{npTrack.Title}` no canal **{manager.Connection.Channel.Mention}**"
                };
                DiscordMessageBuilder msg = new DiscordMessageBuilder().AddEmbed(embedBuilder);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new(msg));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "n to conectado aq" });
            }
        }
    }
}