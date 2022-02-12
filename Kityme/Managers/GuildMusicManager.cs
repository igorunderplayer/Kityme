using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Kityme.Entities;
using Kityme.Extensions;
using Newtonsoft.Json;

namespace Kityme.Managers
{
    public class GuildMusicManager
    {
        public DiscordClient Client { get; private set; }
        public LavalinkGuildConnection Connection { get; set; }
        public LavalinkNodeConnection Node { get; private set; }
        public List<LavalinkTrack> _queue = new();
        public Func<ulong, bool> RemoveThis { get; private set; }
        public List<Filter> _filters = new List<Filter>();
        public bool _loopEnabled = false;
        public bool _shuffleEnabled = false;
        public DiscordMessage LastMessage { get; private set; }
        public int ActualIndex { get; private set; }

        public GuildMusicManager(DiscordClient client, DiscordGuild guild, DiscordChannel channel, Func<ulong, bool> rmThis)
        {
            this.Client = client;
            Node = Client.GetLavalink().GetIdealNodeConnection(channel.RtcRegion);
            RemoveThis = rmThis;
            Connect(guild, channel).Wait();
        }

        private async Task Connect(DiscordGuild guild, DiscordChannel c)
        {
            await Node?.ConnectAsync(c);
            Connection = Node?.GetGuildConnection(guild);
            Connection.PlaybackFinished += Connection_PlaybackFinished;
        }

        public async Task Desconnect(CommandContext ctx)
        {
            if (ctx.Member.VoiceState?.Channel?.Id == Connection.Channel.Id)
            {
                await Connection.DisconnectAsync();
                await ctx.RespondAsync($"desconectei do canal {Connection.Channel.Name}");
            } else
            {
                await ctx.RespondAsync("vc n ta no msm canal de voz q eu");
                return;
            }

        }

        public async Task<PlayResponse> Play(DiscordChannel channel, DiscordMember member, string query, Uri uri = null)
        {
            LavalinkLoadResult loadResult = uri == null ? await Search(query) : await Node.Rest.GetTracksAsync(uri);


            if (loadResult?.LoadResultType == LavalinkLoadResultType.NoMatches || loadResult == null)
                return new PlayResponse(PlayResponseType.TrackNotFound);

            if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
            {
                var tracks = loadResult.Tracks;
                List<LavalinkTrack> _tracks = tracks.ToList();
                foreach (LavalinkTrack _track in _tracks)
                {
                    _track.SetRequester(member);
                    _queue.Add(_track);
                }

                if (_queue.Count == _tracks.Count)
                {
                    LavalinkTrack firstTrack = _queue.First();
                    await Connection.PlayAsync(firstTrack);
                    await SendPlayMessage(firstTrack, channel);
                    ActualIndex = 0;
                    return new PlayResponse(PlayResponseType.PlaylistLoad);
                }
                else return new PlayResponse(PlayResponseType.PlaylistLoad);
            }

            LavalinkTrack track = loadResult.Tracks.First();
            track.SetRequester(member);
            if (_queue.Count == 0)
            {
                _queue.Add(track);
                await Connection.PlayAsync(track);
                await SendPlayMessage(track, channel);
                ActualIndex = 0;
                return new PlayResponse(PlayResponseType.SingleTrackLoad, track);
            } else
            {
                _queue.Add(track);
                return new PlayResponse(PlayResponseType.SingleTrackLoad, track);
            }
        }

        public async Task<bool> Remove(int index)
        {
            if (index <= -1) return false;
            LavalinkTrack remove = _queue.ElementAtOrDefault(index);
            if (remove == null) return false;

            if (ActualIndex == index)
            {
                await Skip();
                _queue.Remove(remove);
                return true;
            }
            _queue.Remove(remove);
            return true;
        }

        public async Task SendPlayMessage(LavalinkTrack track, DiscordChannel c = null)
        {
            
            DiscordMember requester = track.GetRequester();
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"Tocando {track.Title}")
                .WithUrl(track.Uri)
                .WithDescription($"estou tocando {track.Title} no canal {Connection.Channel.Name}")
                .WithColor(DiscordColor.Red)
                .WithFooter($"requested by {requester.Username}#{requester.Discriminator}", requester.AvatarUrl);

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .WithEmbed(embedBuilder)
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Secondary, "m_loop", "", false, new("🔁")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "m_skip_previous", "", false,  new("⏮️")),
                    new DiscordButtonComponent(ButtonStyle.Danger, "m_stop", "", false, new("⏹️")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "m_skip", "", false, new("⏭️")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "m_shuffle", "", false, new("🔀"))
                });

            if (LastMessage != null)
                await LastMessage.DeleteAsync();

            if (c != null)
                LastMessage = await c.SendMessageAsync(messageBuilder);
            else
                LastMessage = await LastMessage?.Channel.SendMessageAsync(messageBuilder);
        }

        public async Task<LavalinkLoadResult> Search (string query)
        {
            LavalinkLoadResult result = await Node.Rest.GetTracksAsync(query);

            if (result.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                return null;

            return result;
        }

        public async Task<bool> Skip (int skipTo = -1)
        {
            if(skipTo == -1)
            {
                LavalinkTrack nextTrack = _shuffleEnabled ? _queue[new Random().Next(0, _queue.Count)] : _queue.ElementAtOrDefault(ActualIndex + 1);
                if (nextTrack != null)
                {
                    await Connection.PlayAsync(nextTrack);
                    await SendPlayMessage(nextTrack);
                    ActualIndex++;
                    return true;
                } else
                {
                    if(_loopEnabled)
                    {
                        nextTrack = _queue.ElementAtOrDefault(0);
                        if(nextTrack != null)
                        {
                            await Connection.PlayAsync(nextTrack);
                            await SendPlayMessage(nextTrack);
                            ActualIndex = 0;
                            return true;
                        }
                    }
                    return false;
                }
            } else
            {
                LavalinkTrack nextTrack = _queue.ElementAtOrDefault(skipTo);
                if (nextTrack != null)
                {
                    await Connection.PlayAsync(nextTrack);
                    await SendPlayMessage(nextTrack);
                    ActualIndex = skipTo;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanChangeQueue(DiscordMember u)
        {
            if (u == null) return false;
            if (this.Connection.Channel != u.VoiceState?.Channel) return false;
            if (_queue[ActualIndex].GetRequester().Id == u.Id) return true;
            if (u.Permissions.HasPermission(Permissions.ManageMessages)) return true;

            DiscordRole djRole = u.Guild.Roles.FirstOrDefault(r => r.Value.Name == "DJ").Value;
            if (djRole == null) return false;
            if (u.Roles.Contains(djRole)) return true;

            return false;
        }

        public async Task<bool> SetFilter(string filter)
        {
            Filter _filter = null;

            if(filter == "reset")
            {
                List<Equalizer> eq = new();
                Timescale tms = new();
                Tremolo tremo = new();
                string reset_message = "{ \"op\": \"filters\", \"guildId\": \"" + Connection.Guild.Id + "\", \"equalizer\": " + eq + ", \"timescale\": " + tms + ", \"tremolo\": " + tremo + " }";
                await Connection.Node.SendAsync(reset_message);
                return true;
            }

            if (filter == "bass")
                _filter = JsonConvert.DeserializeObject<Filter>("{ \"equalizer\":[{\"band\":0,\"gain\":0.6},{\"band\":1,\"gain\":0.67},{ \"band\":2,\"gain\":0.67},{ \"band\":3,\"gain\":0},{ \"band\":4,\"gain\":-0.5},{ \"band\":5,\"gain\":0.15},{ \"band\":6,\"gain\":-0.45},{ \"band\":7,\"gain\":0.23},{ \"band\":8,\"gain\":0.35},{ \"band\":9,\"gain\":0.45},{ \"band\":10,\"gain\":0.55},{ \"band\":11,\"gain\":0.6},{ \"band\":12,\"gain\":0.55},{ \"band\":13,\"gain\":0}]}");

            if (filter == "pop")
                _filter = JsonConvert.DeserializeObject<Filter>("{\"equalizer\":[{\"band\":0,\"gain\":0.65},{\"band\":1,\"gain\":0.45},{\"band\":2,\"gain\":-0.45},{\"band\":3,\"gain\":-0.65},{\"band\":4,\"gain\":-0.35},{\"band\":5,\"gain\":0.45},{\"band\":6,\"gain\":0.55},{\"band\":7,\"gain\":0.6},{\"band\":8,\"gain\":0.6},{\"band\":9,\"gain\":0.6},{\"band\":10,\"gain\":0},{\"band\":11,\"gain\":0},{\"band\":12,\"gain\":0},{\"band\":13,\"gain\":0}]}");

            if (filter == "nightcore")
                _filter = JsonConvert.DeserializeObject<Filter>("{\"equalizer\":[{\"band\":1,\"gain\":0.3},{\"band\":0,\"gain\":0.3}],\"timescale\":{\"pitch\":1.2},\"tremolo\":{\"depth\":0.3,\"frequency\":14}}");

            if (filter == "soft")
                _filter = JsonConvert.DeserializeObject<Filter>("{\"equalizer\":[{\"band\":0,\"gain\":0},{\"band\":1,\"gain\":0},{\"band\":2,\"gain\":0},{\"band\":3,\"gain\":0},{\"band\":4,\"gain\":0},{\"band\":5,\"gain\":0},{\"band\":6,\"gain\":0},{\"band\":7,\"gain\":0},{\"band\":8,\"gain\":-0.25},{\"band\":9,\"gain\":-0.25},{\"band\":10,\"gain\":-0.25},{\"band\":11,\"gain\":-0.25},{\"band\":12,\"gain\":-0.25},{\"band\":13,\"gain\":-0.25}]}");

            if (filter == "vaporwave")
                _filter = JsonConvert.DeserializeObject<Filter>("{\"equalizer\":[{\"band\":1,\"gain\":0.3},{\"band\":0,\"gain\":0.3}],\"timescale\":{\"pitch\":0.5},\"tremolo\":{\"depth\":0.3,\"frequency\":14}}");

            if (filter == "treblebass")
                _filter = JsonConvert.DeserializeObject<Filter>("{\"equalizer\":[{\"band\":0,\"gain\":0.6},{\"band\":1,\"gain\":0.67},{\"band\":2,\"gain\":0.67},{\"band\":3,\"gain\":0},{\"band\":4,\"gain\":-0.5},{\"band\":5,\"gain\":0.15},{\"band\":6,\"gain\":-0.45},{\"band\":7,\"gain\":0.23},{\"band\":8,\"gain\":0.35},{\"band\":9,\"gain\":0.45},{\"band\":10,\"gain\":0.55},{\"band\":11,\"gain\":0.6},{\"band\":12,\"gain\":0.55},{\"band\":13,\"gain\":0}]}");

            if (_filter == null)
                return false;

            Timescale tm = new(_filter.timescale?.pitch is <= 0 or null ? 1f : _filter.timescale.pitch,
                _filter.timescale?.rate is <= 0 or null ? 1.0f : _filter.timescale.rate,
                _filter.timescale?.speed is <= 0 or null ? 1.0f : _filter.timescale.speed
                );

            Tremolo trm = new(_filter.tremolo?.depth is <= 0 or null ? 0.5f : _filter.tremolo.depth,
                _filter.tremolo?.frequency is <= 0 or null ? 2.0f : _filter.tremolo.frequency
                );


            string equalizer = JsonConvert.SerializeObject(_filter.equalizer);
            string timescale = JsonConvert.SerializeObject(tm);
            string tremolo = JsonConvert.SerializeObject(trm);

            Console.WriteLine(equalizer);
            Console.WriteLine(timescale);
            Console.WriteLine(tremolo);


            string message = "{ \"op\": \"filters\", \"guildId\": \"" + Connection.Guild.Id + "\", \"equalizer\": "+ equalizer +", \"timescale\": "+ timescale +", \"tremolo\": " + tremolo + " }";
            await Connection.Node.SendAsync(message);
            return true;
        }
        private async Task Connection_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            if(e.Reason == TrackEndReason.Finished)
            {
                ulong guildId = sender.Guild.Id;
                if ((_queue.Count - (ActualIndex + 1)) <= 0)
                {
                    if (_loopEnabled)
                    {
                        await sender.PlayAsync(_queue[0]);
                        await SendPlayMessage(_queue[0]);
                        ActualIndex = 0;
                        return;
                    }
                    await LastMessage.DeleteAsync();
                    await sender.DisconnectAsync();
                    RemoveThis(sender.Guild.Id);
                    await LastMessage.Channel.SendMessageAsync("todas musicas da fila acabaram!");
                }
                else
                {
                    int nextTrackIndex = _shuffleEnabled ? new Random().Next(0, _queue.Count) : ActualIndex + 1;
                    LavalinkTrack nextTrack = _queue.ElementAtOrDefault(nextTrackIndex);
                    if (nextTrack != null)
                    {
                        await sender.PlayAsync(nextTrack);
                        await SendPlayMessage(nextTrack);
                        ActualIndex = nextTrackIndex;
                    }
                }
                return;
            }
            
        }
    }

    public class PlayResponse
    {
        public PlayResponseType Type { get; private set; }
        public LavalinkTrack Track { get; private set; }
        public PlayResponse (PlayResponseType type, LavalinkTrack track = null)
        {
            this.Type = type;
            this.Track = track;
        }
    }

    public enum PlayResponseType
    {
        PlaylistLoad,
        SingleTrackLoad,
        TrackNotFound
    }
}