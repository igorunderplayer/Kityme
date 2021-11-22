using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kityme.Commands
{
    class FunCommands : BaseCommandModule
    {

        [Command("avatar"), Description("rouba avatar dos outros hiihihihi")]
        public async Task Avatar(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Avataro neh meu",
                ImageUrl = ctx.User.GetAvatarUrl(DSharpPlus.ImageFormat.Auto, 2048),
                Color = DiscordColor.Purple
            };

            await ctx.RespondAsync(embed);
        }
        [Command("avatar"), Description("rouba avatar dos outros hiihihihi")]
        public async Task Avatar(CommandContext ctx, DiscordMember member)
        {

            DiscordEmbed embed = new DiscordEmbedBuilder
            {
                Title = "Avataro neh meu",
                ImageUrl = member.GetAvatarUrl(DSharpPlus.ImageFormat.Auto, 2048),
                Color = DiscordColor.Lilac
            };

            await ctx.RespondAsync(embed);
        }


        [Command("bomdia")]
        public async Task bomdia(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Bom dia meu amigo, como vai vc, bem?, que bom entao");
        }

        [Command("gaytest"), Aliases("testegay", "testgay")]
        public async Task Gaytest(CommandContext ctx)
        {
            int num = new Random().Next(0, 100);
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Gaytest", IconUrl = ctx.User.AvatarUrl },
                Description = $"Tu eh {num}% gay",
                Color = DiscordColor.HotPink,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "muito gay mano........." }
            };

            await ctx.RespondAsync(embed);
        }

        [Command("destino")]
        public async Task Destino(CommandContext ctx)
        {
            ulong[] discordKeys = ctx.Guild.Members.Keys.ToArray();
            ulong key = (ulong)new Random().Next(0, discordKeys.Length);
            ulong memberID = discordKeys[key];
            DiscordMember member = await ctx.Guild.GetMemberAsync(memberID);

            string[] destinos =
            {
            "casar comigo",
            "me dar um beijo",
            $"dar a bunda para {member.Mention}",
            $"banir {member.Mention}",
            "ser banido",
            "ser promovido",
            "virar adeeme",
            "perder adeeme",
            "ser rebaixado",
            $"dar uma mamada em {member.Mention}",
            "me adicionar no seu servidor",
            "ficar famoso",
            "me dar adm",
            "me dar uma mamada",
            "virar femboy",
            "fazer um filme porno",
            "virar artista hentai",
            "me dar seu numero",
            "me dar nitro",
            $"dar nitro para {member.Mention}",
            "dar nitro para meu criador",
            "assistir hentai comigo",
            "ficar milionario",
            "tropeçar",
            "ganhar um pc gamer",
            "sair do armario",
            "ficar pobre",
            "morrer",
            "transcender",
            $"dar um tapa em {member.Mention}",
            $"receber um tapa de {member.Mention}",
            "ficar careca",
            "beber agua",
            "mandar nsfw no #geral",
            "virar gay",
            "virar mulhe",
            "virar homi",
            "virar lgtv",
            "ganhar na loteria",
            "ficar burro",
            "decifrar meu enigma",
            "participar de um rpg",
            "cair da escada",
            "cair",
            "virar deus de um novo mundo",
            "nenhum",
            "achar um caderno estranho",
            "virar otaku",
            "virar trap",
            $"comer a bunda de {member.Mention}",
            "criar um servidor e me adicionar",
            "ser travado no zap",
            $"travar o zap de {member.Mention}",
            $"ser travado por {member.Mention}",
            $"ganhar o numero de {member.Mention}",
            "fazer websexu",
            $"fazer websexu com {member.Mention}"
            };

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "descubra seu destino 🗿", IconUrl = ctx.User.AvatarUrl },
                Description = $"Seu destino eh.... \n\n{destinos[new Random().Next(0, destinos.Length)]}",
                Color = DiscordColor.HotPink,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "100% real man" }
            };
            await ctx.RespondAsync(embed.Build());
        }

        [Command("emergencymeeting")]
        public async Task EmergencyMeeting(CommandContext ctx, params string[] args)
        {
            //https://vacefron.nl/api/emergencymeeting?text=gay
            string text = string.Join("+", args);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red)
                .WithImageUrl($"https://vacefron.nl/api/emergencymeeting?text={text}");

            await ctx.RespondAsync(embed);
        }

        [Command("stonks")]
        public async Task Stonks (CommandContext ctx, [RemainingText] DiscordMember member)
        {
            string userAvatar = member == null ? ctx.User.GetAvatarUrl(DSharpPlus.ImageFormat.Png) : member.GetAvatarUrl(DSharpPlus.ImageFormat.Png);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Blue)
                .WithImageUrl($"https://vacefron.nl/api/stonks?user={userAvatar}");

            await ctx.RespondAsync(embed);
        }

        [Command("notstonks")]
        public async Task NotStonks(CommandContext ctx, [RemainingText] DiscordMember member)
        {
            string userAvatar = member == null ? ctx.User.GetAvatarUrl(DSharpPlus.ImageFormat.Png) : member.GetAvatarUrl(DSharpPlus.ImageFormat.Png);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithImageUrl($"https://vacefron.nl/api/stonks?user={userAvatar}&notstonks=true");

            await ctx.RespondAsync(embed);
        }

        [Command("changemymind")]
        public async Task ChangeMyMind (CommandContext ctx, params string[] args)
        {

            string text = string.Join("+", args);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.White)
                .WithImageUrl($"https://vacefron.nl/api/changemymind?text={text}");

            await ctx.RespondAsync(embed);
        }
    }
}
