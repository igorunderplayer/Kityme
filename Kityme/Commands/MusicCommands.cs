using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kityme.Managers;

namespace Kityme.Commands
{
    class MusicCommands : BaseCommandModule
    {
        [Command("join")]
        public async Task Join(CommandContext ctx, DiscordChannel channel = null)
        {

            DiscordVoiceState voiceState = ctx.Member?.VoiceState;

            if (voiceState.Channel == null && channel == null)
            {
                await ctx.RespondAsync("vc n ta em um canal de voz");
                return;
            }

            channel ??= voiceState.Channel;

            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Lavalink n ta conectado");
                return;
            }

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Anal invalido");
                return;
            }

            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                await ctx.RespondAsync("ja to num canal d voz");
            }
            else
            {
                MusicManagers._managers.Add(ctx.Guild.Id, new GuildMusicManager(ctx.Client, ctx.Guild, channel, (id) => MusicManagers._managers.Remove(id)));
            }
        }

        [Command("leave"), Aliases("dc", "disconnect", "stop")]
        public async Task Leave(CommandContext ctx)
        {

            DiscordVoiceState voiceState = ctx.Member.VoiceState;

            if (voiceState?.Channel == null)
            {
                await ctx.RespondAsync("vc n ta em um canal de voz");
                return;
            }


            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Lavalink n ta conectado");
                return;
            }

            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                if (!MusicManagers._managers[ctx.Guild.Id].CanChangeQueue(ctx.Member))
                {
                    await ctx.RespondAsync("n pode'-");
                    return;
                }
                await MusicManagers._managers[ctx.Guild.Id].Desconnect(ctx);
                MusicManagers._managers.Remove(ctx.Guild.Id);
                return;
            } else
            {
                await ctx.RespondAsync("n to conectado aqui");
                return;
            }

        }

        [Command("play"), Aliases("p")]
        public async Task Play(CommandContext ctx, Uri uri)
        {
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;
            DiscordChannel channel = voiceState?.Channel;

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("se precisa ta num canal de voz '-'");
                return;
            }

            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Lavalink n ta conectado");
                return;
            }

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Anal invalido");
                return;
            }

            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                if (ctx.Member.VoiceState.Channel.Id != MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id)
                {
                    await ctx.RespondAsync("ja to num canal d voz");
                    return;
                }
            }
            else
            {
                MusicManagers._managers.Add(ctx.Guild.Id, new GuildMusicManager(ctx.Client, ctx.Guild, channel, (id) => MusicManagers._managers.Remove(id)));
            }
            await MusicManagers._managers[ctx.Guild.Id].Play(ctx.Channel, ctx.Member, null, uri);
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("se precisa ta num canal de voz '-'");
                return;
            }
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;
            DiscordChannel channel = voiceState.Channel;


            LavalinkExtension lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Lavalink n ta conectado");
                return;
            }

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Anal invalido");
                return;
            }

            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                if (ctx.Member.VoiceState.Channel.Id != MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id)
                {
                    await ctx.RespondAsync("ja to num canal d voz");
                    return;
                }
            }
            else
            {
                MusicManagers._managers.Add(ctx.Guild.Id, new GuildMusicManager(ctx.Client, ctx.Guild, channel, (id) => MusicManagers._managers.Remove(id)));
            }
            PlayResponseType res = await MusicManagers._managers[ctx.Guild.Id].Play(ctx.Channel, ctx.Member, search);
            if (res == PlayResponseType.SingleTrackLoad)
                await ctx.RespondAsync($"track adicionada a fila '-");
            else if (res == PlayResponseType.PlaylistLoad)
                await ctx.RespondAsync("playlist carregada -");
            else if (res == PlayResponseType.TrackNotFound)
                await ctx.RespondAsync("n achei");
        }

        [Command("nowplaying"), Aliases("np")]
        public async Task NowPlaying(CommandContext ctx)
        {
            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                GuildMusicManager manager = MusicManagers._managers[ctx.Guild.Id];
                LavalinkTrack npTrack = manager._queue[manager.actualIndex];

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Now Playing",
                    Description = $"Atualmente estou tocando `{npTrack.Title}` no canal **{manager.Connection.Channel.Mention}**"
                };

                await ctx.RespondAsync(embedBuilder);

            } else
            {
                await ctx.RespondAsync("n estou conectado aqui");
            }
        }

        [Command("skip"), Aliases("s")]
        public async Task Skip (CommandContext ctx, int skipTo = 0)
        {
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;

            if(skipTo < 0)
            {
                await ctx.RespondAsync("o valor n pode se negativo '-");
                return;
            }

            if (voiceState == null || voiceState.Channel == null)
            {
                await ctx.RespondAsync("se precisa ta num canal de voz '-'");
                return;
            }


            if(!MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                await ctx.RespondAsync("n to conectado em nenhum canal");
                return;
            }

            if(MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id != voiceState.Channel.Id)
            {
                await ctx.RespondAsync("vc precisa ta no msm canal q eu'-");
                return;
            }

            if (!MusicManagers._managers[ctx.Guild.Id].CanChangeQueue(ctx.Member))
            {
                await ctx.RespondAsync("n pode'-");
                return;
            }

            bool succes = await MusicManagers._managers[ctx.Guild.Id].Skip(skipTo == 0 ? -1 : skipTo - 1);
            if(!succes)
            {
                await ctx.RespondAsync("n foi possivel pular, certifique-se de colocar o numero certo ou se tem outra musica na fila");
            } else
            {
                await ctx.RespondAsync("pulado!");
            }
        }

        [Command("remove"), Aliases("rm")]
        public async Task Remove (CommandContext ctx, int index)
        {
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;

            if (index < 0)
            {
                await ctx.RespondAsync("o valor n pode se negativo '-");
                return;
            }

            if (voiceState == null || voiceState.Channel == null)
            {
                await ctx.RespondAsync("se precisa ta num canal de voz '-'");
                return;
            }


            if (!MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                await ctx.RespondAsync("n to conectado em nenhum canal");
                return;
            }

            if (MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id != voiceState.Channel.Id)
            {
                await ctx.RespondAsync("vc precisa ta no msm canal q eu'-");
                return;
            }
            if (!MusicManagers._managers[ctx.Guild.Id].CanChangeQueue(ctx.Member))
            {
                await ctx.RespondAsync("n pode'-");
                return;
            }
            bool succes = await MusicManagers._managers[ctx.Guild.Id].Remove(index - 1);
            if (!succes)
            {
                await ctx.RespondAsync("n foi possivel remover, certifique-se de colocar o numero valido ou que exista na fila");
            }
            else
            {
                await ctx.RespondAsync("removido!");
            }
        }

        [Command("loop"), Aliases("l", "lq")]
        public async Task Loop(CommandContext ctx)
        {
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;

            if (voiceState == null || voiceState.Channel == null)
            {
                await ctx.RespondAsync("se precisa ta num canal de voz '-'");
                return;
            }


            if (!MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                await ctx.RespondAsync("n to conectado em nenhum canal");
                return;
            }

            if (MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id != voiceState.Channel.Id)
            {
                await ctx.RespondAsync("vc precisa ta no msm canal q eu'-");
                return;
            }

            GuildMusicManager manager = MusicManagers._managers[ctx.Guild.Id];
            if (!manager.CanChangeQueue(ctx.Member))
            {
                await ctx.RespondAsync("n pode'-");
                return;
            }
            MusicManagers._managers[ctx.Guild.Id]._loopEnabled = !manager._loopEnabled;

            string msg = MusicManagers._managers[ctx.Guild.Id]._loopEnabled ? "ativado" : "desativado";

            await ctx.RespondAsync($"loop {msg}");
        }

        [Command("seek")]
        public async Task Seek(CommandContext ctx, TimeSpan time)
        {
            await ctx.RespondAsync(time.TotalMinutes.ToString());
        }

        [Command("shuffle"), Aliases("sf", "aleatorizar")]
        public async Task Shuffle(CommandContext ctx)
        {
            DiscordVoiceState voiceState = ctx.Member?.VoiceState;

            if (voiceState == null || voiceState.Channel == null)
            {
                await ctx.RespondAsync("se precisa ta num canal de voz '-'");
                return;
            }


            if (!MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                await ctx.RespondAsync("n to conectado em nenhum canal");
                return;
            }

            if (MusicManagers._managers[ctx.Guild.Id].Connection.Channel.Id != voiceState.Channel.Id)
            {
                await ctx.RespondAsync("vc precisa ta no msm canal q eu'-");
                return;
            }

            GuildMusicManager manager = MusicManagers._managers[ctx.Guild.Id];
            if (!manager.CanChangeQueue(ctx.Member))
            {
                await ctx.RespondAsync("n pode'-");
                return;
            }
            MusicManagers._managers[ctx.Guild.Id]._shuffleEnabled = !manager._shuffleEnabled;

            string msg = MusicManagers._managers[ctx.Guild.Id]._shuffleEnabled ? "ativado" : "desativado";

            await ctx.RespondAsync($"shuffle {msg}");
        }

        [Command("filter")]
        public async Task Filter (CommandContext ctx, [RemainingText] string filter)
        {
            await ctx.RespondAsync("so avisando q esse cmd n ta pronto'-");
            if (!MusicManagers._managers[ctx.Guild.Id].CanChangeQueue(ctx.Member))
            {
                await ctx.RespondAsync("n pode'-");
                return;
            }
            bool res = await MusicManagers._managers[ctx.Guild.Id].SetFilter(filter);
            if (res)
                await ctx.RespondAsync("filtro aplicado com sucesso!");
            else
                await ctx.RespondAsync("n deu d aplica n");
        }

        [Command("queue"), Aliases("q")]
        public async Task Queue (CommandContext ctx, [RemainingText] int index = 1)
        {
            if(index <= 0)
            {
                await ctx.RespondAsync("numero precisa ser maior q 0'-");
                return;
            }
            if (MusicManagers._managers.ContainsKey(ctx.Guild.Id))
            {
                string q = string.Empty;
                GuildMusicManager manager = MusicManagers._managers[ctx.Guild.Id];
                for (int i = 0; i < manager._queue.Count; i++)
                {
                    q += $"{i + 1} - {manager._queue[i].Title}\n";
                }

                var interactivity = ctx.Client.GetInteractivity();
                var pages = interactivity.GeneratePagesInEmbed(q);

                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
            } else
            {
                await ctx.RespondAsync("n estou conectado aqui");
            }
        }

    }
}
