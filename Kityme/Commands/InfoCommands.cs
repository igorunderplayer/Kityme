using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Kityme.Attributes;
using System.Net.Http;
using Newtonsoft.Json;

namespace Kityme.Commands
{
    public class InfoCommands: BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping (CommandContext ctx)
        {
            var ping = ctx.Client.Ping;
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor("Ping?? Pega no meu pong aqui oh", ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.Cyan)
                .WithDescription($"meu ping é {ping}ms");

            await ctx.RespondAsync(embed);
        }


        [Command("fox")]
        public async Task Fox (CommandContext ctx)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://randomfox.ca");
                HttpResponseMessage response = await client.GetAsync("/floof");
                if (response.IsSuccessStatusCode)
                {
                    var fox = await response.Content.ReadAsStringAsync();
                    Fox data = JsonConvert.DeserializeObject<Fox>(fox);

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                        .WithTitle("raposious")
                        .WithColor(DiscordColor.Orange)
                        .WithImageUrl(data.Image);

                    await ctx.RespondAsync(embedBuilder);
                }
            }
        }

        [Command("invite"), Aliases("convite"), Description("manda link pra me add")]
        public async Task Invite (CommandContext ctx)
        {
            string invite = $"https://discord.com/oauth2/authorize?client_id={ctx.Client.CurrentUser.Id}&scope=bot+applications.commands&permissions=75361473";
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("me adicione")
                .WithDescription($"me adicionando clicando [aqui]({invite})");
            await ctx.RespondAsync(embedBuilder);
        }

        [Command("lavalink"), Description("mostra informações do lavalink '-")]
        public async Task Lavalink (CommandContext ctx)
        {
            LavalinkExtension lavalink = ctx.Client.GetLavalink();
            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();

            long ramUsed = node.Statistics.RamUsed / 1024 / 1024;
            var uptime = node.Statistics.Uptime;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Lavalink",
                Description = $"**RAM**: {ramUsed} \n **Uptime**: {uptime}"
            };

            await ctx.RespondAsync(embed);

        }

        [Command("botinfo"), Description("mostra minhas informações"), Aliases("bi", "stats"), CommandType("Info")]
        public async Task Botinfo (CommandContext ctx)
        {
            Process p = Process.GetCurrentProcess();
            long ram = p.WorkingSet64 / 1024 / 1024;
            ulong id = 477534823011844120;

            DiscordUser owner = await ctx.Client.GetUserAsync(id);
            var time = DateTime.UtcNow - p.StartTime.ToUniversalTime();
            var threads = p.Threads.Count;
            

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Minhas informações",
                Color = DiscordColor.Blurple,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Criado por {owner.Username}", IconUrl = owner.AvatarUrl },
                Description = $"🌐 | Servidores: {ctx.Client.Guilds.Count} \n" +
                              $"🏓 | Ping: {ctx.Client.Ping}ms \n" +
                              $"📁 | Total de comandinhos: {ctx.Client.GetCommandsNext().RegisteredCommands.Count} \n" +
                              "\n" +
                              $"📈 | RAM: {ram}MB \n" +
                              $"Uptime(tavez): {time.ToString("h'h 'm'm 's's'")} \n" +
                              $"Threads(tavez tbm): {threads}"

            };

            await ctx.RespondAsync(embed);
        }

        [Command("tabuada"), Description("mostra a tabuada d um numero")]
        public async Task Tabuada(CommandContext ctx, double num)
        {
            if (Double.IsNaN(num) || Double.IsInfinity(num))
            {
                await ctx.RespondAsync("'- ai n neh man");
                return;
            }

            string tabuada = String.Empty;

            for (int i = 0; i < 11; i++)
            {
                double val = num * i;
                tabuada += $"{num} * {i} = {val} \n";
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"Tabuda de {num}",
                Description = tabuada,
                Color = DiscordColor.LightGray
            };

            await ctx.RespondAsync(embed);
        }
    }

    public struct Fox
    {
        [JsonProperty] public string Image { get; set; }
        [JsonProperty] public string Link { get; set; }
    }
}
