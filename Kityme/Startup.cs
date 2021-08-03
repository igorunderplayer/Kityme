using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Kityme.Commands;
using Kityme.Commands.SlashCommands;
using Kityme.Entities;
using Kityme.Events.Client;
using Kityme.Extensions;
using Kityme.Managers;
using Newtonsoft.Json;

namespace Kityme
{
    public class Startup
    {
        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension SlashCommands { get; private set; }
        public LavalinkExtension Lavalink { get; set; }
        public ConfigJson CfgJson;

        public async Task RunAsync()
        {
            string json;
            await using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            CfgJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = CfgJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                UseRelativeRatelimit = true,

                Intents = DiscordIntents.Guilds
                          | DiscordIntents.GuildMessages
                          | DiscordIntents.GuildMessageReactions
                          | DiscordIntents.GuildMembers
                          | DiscordIntents.GuildBans
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

                User u = await e.Author.GetAsync();
                if (u == null) await e.Author.RegistUserAsync();
                return;
            };

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = CfgJson.Prefixes,
                EnableMentionPrefix = true,
                EnableDms = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<InfoCommands>();
            Commands.RegisterCommands<DevCommands>();
            Commands.RegisterCommands<RPCommands>();
            Commands.RegisterCommands<MusicCommands>();
            Commands.RegisterCommands<EconomyCommands>();

            SlashCommands = Client.UseSlashCommands();

            SlashCommands.RegisterCommands<MusicSlashCommands>();


            string host = CfgJson.LavalinkHost;
            HttpClient client = new HttpClient();
            var res = await client.GetAsync("https://" + host);
            if (res.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                host = CfgJson.SLavalinkHost;

            ConnectionEndpoint endpoint = new ConnectionEndpoint
            {
                Hostname = host,
                Port = 80,
                Secured = false
            };

            LavalinkConfiguration lavalinkConfig = new LavalinkConfiguration
            {
                Password = CfgJson.Lavalink_Password,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint,
                SocketAutoReconnect = true
            };


            Lavalink = Client.UseLavalink();
            Client.UseInteractivity(new()
            {
                Timeout = TimeSpan.FromSeconds(30)
            });

            DBManager.Connect(CfgJson.MongoUrl);

            await Client.ConnectAsync();
            try
            {
                LavalinkNodeConnection node = await Lavalink.ConnectAsync(lavalinkConfig);
            }
            catch (Exception)
            {
            }

            await Task.Delay(-1);
        }

        private async Task Client_ComponentInteractionCreated(DiscordClient sender,
            ComponentInteractionCreateEventArgs e)
        {
            Console.WriteLine("clicked");
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            if (!MusicManagers._managers.ContainsKey(e.Guild.Id))
                return;

            GuildMusicManager manager = MusicManagers._managers[e.Guild.Id];
            if (!manager.CanChangeQueue(e.Guild.Members.GetValueOrDefault(e.User.Id))) return;
            switch (e.Id)
            {
                case "m_skip":
                    await MusicManagers._managers[e.Guild.Id].Skip(-1);
                    break;

                case "m_skip_previous":
                    await MusicManagers._managers[e.Guild.Id].Skip(MusicManagers._managers[e.Guild.Id].actualIndex - 1);
                    break;

                case "m_loop":
                    manager._loopEnabled = !manager._loopEnabled;
                    string loop_pmsg = manager._loopEnabled ? "ativado" : "desativado";
                    await e.Channel.SendMessageAsync($"loop {loop_pmsg}");
                    //await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"loop {msg}"));
                    break;

                case "m_shuffle":
                    manager._shuffleEnabled = !manager._shuffleEnabled;
                    string shuffle_msg = manager._shuffleEnabled ? "ativado" : "desativado";
                    await e.Channel.SendMessageAsync($"embaralhamento {shuffle_msg}");
                    //await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"loop {msg}"));
                    break;

                case "m_stop":
                    await manager.lastMessage?.DeleteAsync();
                    await manager.Connection.DisconnectAsync();
                    manager.removeThis(e.Guild.Id);
                    await e.Channel.SendMessageAsync("sai do canal!");
                    //await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("sai do canal!"));
                    break;
            }

            return;
        }

        private async Task Client_MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Message.Content.StartsWith($"<@{client.CurrentUser.Id}>") ||
                e.Message.Content.StartsWith($"<!@{client.CurrentUser.Id}>"))
                await e.Message.RespondAsync("'- se quise ve meus comando usa k!help 👍");

            User u = await e.Author.GetAsync();
            if (u == null) await e.Author.RegistUserAsync();
            return;
        }


        public struct ConfigJson
        {
            [JsonProperty("token")] public string Token { get; private set; }

            [JsonProperty("mongo_url")] public string MongoUrl { get; private set; }

            [JsonProperty("lavalink_host")] public string LavalinkHost { get; private set; }

            [JsonProperty("slavalink_host")] public string SLavalinkHost { get; private set; }

            [JsonProperty("lavalink_password")] public string Lavalink_Password { get; private set; }

            [JsonProperty("prefixes")] public string[] Prefixes { get; private set; }
        }

    }
}