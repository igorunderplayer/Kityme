using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Kityme.Commands;
using Kityme.Commands.SlashCommands;
using Kityme.Entities;
using Kityme.Events.Client;
using Kityme.Extensions;
using Kityme.Managers;
using Kityme.Utils;
using Newtonsoft.Json;

namespace Kityme
{
    public class Startup
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension SlashCommands { get; private set; }
        public LavalinkExtension Lavalink { get; private set; }
        private BotConfig botConfig;

        public async Task RunAsync()
        {
            string root = Directory.GetCurrentDirectory();
            string dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            botConfig = new BotConfig(
                Environment.GetEnvironmentVariable("TOKEN"),
                Environment.GetEnvironmentVariable("LAVALINK_HOST"),
                Environment.GetEnvironmentVariable("SLAVALINK_HOST"),
                Environment.GetEnvironmentVariable("LAVALINK_PASSWORD"),
                Environment.GetEnvironmentVariable("MONGO_URL")
            );

            var config = new DiscordConfiguration
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                UseRelativeRatelimit = true,

                Intents = DiscordIntents.Guilds
                          | DiscordIntents.GuildMessages
                          | DiscordIntents.GuildVoiceStates
            };


            Client = new DiscordClient(config);

            Client.Ready += new Ready(Client).Client_Ready;
            Client.VoiceStateUpdated += new VoiceStateUpdated(Client).Client_VoiceStateUpdated;
            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            Client.MessageCreated += async (DiscordClient client, MessageCreateEventArgs e) =>
            {
                if (e.Message.Content.StartsWith($"<@{client.CurrentUser.Id}>") ||
                    e.Message.Content.StartsWith($"<!@{client.CurrentUser.Id}>"))
                    await e.Message.RespondAsync("'- se quise ve meus comando usa k!help 👍");
                
                return;
            };

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = new string[] { "k!", "kityme" },
                EnableMentionPrefix = true,
                EnableDms = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            /// Commands.SetHelpFormatter<KitymeHelpFormatter>();

                Commands.RegisterCommands<ModCommands>();
                Commands.RegisterCommands<FunCommands>();
                Commands.RegisterCommands<InfoCommands>();
                Commands.RegisterCommands<DevCommands>();
                Commands.RegisterCommands<RPCommands>();
                Commands.RegisterCommands<MusicCommands>();
                Commands.RegisterCommands<EconomyCommands>();

            SlashCommands = Client.UseSlashCommands();

                SlashCommands.RegisterCommands<MusicSlashCommands>();
                SlashCommands.RegisterCommands<TestingSlashCommands>(720709045119352862); // Guild para testes de slash commands


            ConnectionEndpoint[] endpoint = new[]
            {
                new ConnectionEndpoint()
                {
                    Hostname = botConfig.LavalinkHost,
                    Port = 80,
                    Secured = false
                },
                new ConnectionEndpoint()
                {
                    Hostname = botConfig.SLavalinkHost,
                    Port = 80,
                    Secured = false
                }
            };

            LavalinkConfiguration[] lavalinkConfig = new[]
            {
                new LavalinkConfiguration()
                {
                    Password = botConfig.LavalinkPassword,
                    RestEndpoint = endpoint[0],
                    SocketEndpoint = endpoint[0],
                    SocketAutoReconnect = true
                },
                new LavalinkConfiguration()
                {
                    Password = botConfig.LavalinkPassword,
                    RestEndpoint = endpoint[1],
                    SocketEndpoint = endpoint[1],
                    SocketAutoReconnect = true
                }
            };


            Lavalink = Client.UseLavalink();
            Client.UseInteractivity(new()
            {
                Timeout = TimeSpan.FromSeconds(30)
            });

            DBManager.Connect(botConfig.MongoUrl);

            await Client.ConnectAsync();
            foreach (LavalinkConfiguration lavalinkConfiguration in lavalinkConfig)
            {
                try
                {
                    await Lavalink.ConnectAsync(lavalinkConfiguration);
                }
                catch (Exception)
                {
                    Console.WriteLine("Não foi possivel conectar ao lavalink");
                }
            }

            Lavalink.NodeDisconnected += Lavalink_NodeDisconnected;

            await Task.Delay(-1);
        }

        private async Task Lavalink_NodeDisconnected(LavalinkNodeConnection sender, NodeDisconnectedEventArgs e)
        {
            int i = 0;
            while ((i < 5) && !sender.IsConnected)
            {
                Thread.Sleep(5000 * (i + 1));
                i++;
                await Lavalink.ConnectAsync(new()
                {
                    Password = botConfig.LavalinkPassword,
                    RestEndpoint = sender.NodeEndpoint,
                    SocketEndpoint = sender.NodeEndpoint,
                    SocketAutoReconnect = true
                });
            }
        }

        private async Task Client_ComponentInteractionCreated(DiscordClient sender,
            ComponentInteractionCreateEventArgs e)
        {
            if (!MusicManagers._managers.ContainsKey(e.Guild.Id))
                return;

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildMusicManager manager = MusicManagers._managers[e.Guild.Id];
            if (!manager.CanChangeQueue(e.Guild.Members.GetValueOrDefault(e.User.Id))) return;
            switch (e.Id)
            {
                case "m_skip":
                    await MusicManagers._managers[e.Guild.Id].Skip(-1);
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"pulado! *(pedido por {e.User.Username})*"));
                    break;

                case "m_skip_previous":
                    await MusicManagers._managers[e.Guild.Id].Skip(MusicManagers._managers[e.Guild.Id].ActualIndex - 1);
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"pulado (soq para anterior)! *(pedido por {e.User.Username})*"));
                    break;

                case "m_loop":
                    manager._loopEnabled = !manager._loopEnabled;
                    string loop_pmsg = manager._loopEnabled ? "ativado" : "desativado";
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"loop {loop_pmsg} *(pedido por {e.User.Username})*"));
                    break;

                case "m_shuffle":
                    manager._shuffleEnabled = !manager._shuffleEnabled;
                    string shuffle_msg = manager._shuffleEnabled ? "ativado" : "desativado";
                    //await e.Channel.SendMessageAsync($"embaralhamento {shuffle_msg} *(pedido por {e.User.Username})*");
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"embaralhamento {shuffle_msg} *(pedido por {e.User.Username})*"));
                    break;

                case "m_stop":
                    await manager.LastMessage?.DeleteAsync();
                    await manager.Connection.DisconnectAsync();
                    manager.RemoveThis(e.Guild.Id);
                    //await e.Channel.SendMessageAsync($"sai do canal! *(pedido por {e.User.Username})*");
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"sai do canal! *(pedido por {e.User.Username})*"));
                    break;

                case "sm_select":
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach(string value in e.Values)
                    {
                        // LavalinkTrack track = JsonConvert.DeserializeObject<LavalinkTrack>(jsonTrack);
                        var res = await manager.Node.Rest.GetTracksAsync(value);
                        LavalinkTrack track = res.Tracks.First();
                        manager._queue.Add(track);
                        stringBuilder.Append($" {track.Title} |");
                    }
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"{stringBuilder} adicionado na fila *(pedido por {e.User.Username})*"));
                    break;
            }

            return;
        }
    }
}
