using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Kityme.Commands
{
    class InfoCommands: BaseCommandModule
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

        [Command("botinfo"), Description("mostra minhas informações"), Aliases("bi", "stats")]
        public async Task Botinfo (CommandContext ctx)
        {
            Process p = Process.GetCurrentProcess();
            long ram = p.WorkingSet64 / 1024 / 1024;
            long ram2 = p.PrivateMemorySize64 / 1024 / 1024;
            ulong id = 477534823011844120;

            DiscordUser owner = await ctx.Client.GetUserAsync(id);

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
                              $"📈 | RAM2 (n sei qual ta certo): {ram2}MB"
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
}
