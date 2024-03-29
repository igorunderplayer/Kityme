﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SixLabors.ImageSharp;

namespace Kityme.Commands
{
    class FunCommands : BaseCommandModule
    {

        [Command("slidepuzzle")]
        public async Task SlidePuzzle(CommandContext ctx, ushort size = 3)
        {
            if (ctx.User.Id != 477534823011844120)
            {
                await ctx.RespondAsync("desativado 😞");
                return;
            }

            var game = Minigames.SlidePuzzle.Create(ctx.Client, ctx.Member, ctx.Channel, size);

            if (game == null)
            {
                await ctx.RespondAsync("n foi possivel criar seu jogo, se vc ja estiver dentro de um jogo, utilize k!slidepuzzle cancel para cancela-lo");
                return;
            }
            await game.Start();
            await game.UpdateMessage();
        }

        [Command("slidepuzzle")]
        public async Task SlidePuzzle(CommandContext ctx, string action)
        {
            if (ctx.User.Id != 477534823011844120)
            {
                await ctx.RespondAsync("desativado 😞");
                return;
            }

            if (action is "cancel" or "desistir")
            {
                if (!Managers.Minigames.SlidePuzzle.ContainsKey(ctx.User.Id))
                {
                    await ctx.RespondAsync("vc nem ta jogando man");
                    return;
                }
                Managers.Minigames.SlidePuzzle.Remove(ctx.User.Id);
                await ctx.RespondAsync("vc desistiu do jogo, vc é fraco");
                return;
            }
        }

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
        public async Task Gaytest(CommandContext ctx, [RemainingText] DiscordMember member = null)
        {
            member ??= ctx.Member;
            string tu = member.Id == ctx.User.Id ? "tu" : member.Username;
            int num = new Random().Next(0, 100);
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Gaytest", IconUrl = member.AvatarUrl },
                Description = $"{tu} eh {num}% gay",
                Color = DiscordColor.HotPink,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "muito gay mano........." }
            };

            await ctx.RespondAsync(embed);
        }

        [Command("rps")]
        public async Task RPS(CommandContext ctx, [RemainingText] DiscordMember member = null)
        {
            member ??= ctx.Guild.CurrentMember;

            if (member.Id == ctx.Member.Id)
            {
                await ctx.RespondAsync("ue");
                return;
            }

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .WithContent("aperta ae")
                .AddComponents(new[]{
                 new DiscordButtonComponent(ButtonStyle.Primary, "rps_0", "", false, new("👊")),
                 new DiscordButtonComponent(ButtonStyle.Primary, "rps_1", "", false, new("🖐️")),
                 new DiscordButtonComponent(ButtonStyle.Primary, "rps_2", "", false, new("✌️"))
            });

            await ctx.RespondAsync(messageBuilder);

            string player1Choice = null;
            string player2Choice = null;

            if (member.Id == ctx.Client.CurrentUser.Id)
            {
                player2Choice = new Random().Next(0, 3).ToString();
                Console.WriteLine($"-=-=-=-=-= Escolhido pelo BOT: {player2Choice}");
            }

            ctx.Client.ComponentInteractionCreated += OnInteract;

            async Task EndGame()
            {
                ctx.Client.ComponentInteractionCreated -= OnInteract;
                if (player1Choice == player2Choice)
                {
                    await ctx.RespondAsync("Empate!");
                    return;
                }

                switch (player1Choice)
                {
                    case "0":
                        if (player2Choice == "1")
                        {
                            await ctx.RespondAsync($"{member.Mention} ganhou!");
                        }
                        else if (player2Choice == "2")
                        {
                            await ctx.RespondAsync($"{ctx.Member.Mention} ganhou!");
                        }
                        break;

                    case "1":
                        if (player2Choice == "0")
                        {
                            await ctx.RespondAsync($"{ctx.Member.Mention} ganhou!");
                        }
                        else if (player2Choice == "2")
                        {
                            await ctx.RespondAsync($"{member.Mention} ganhou!");
                        }
                        break;

                    case "2":
                        if (player2Choice == "1")
                        {
                            await ctx.RespondAsync($"{ctx.Member.Mention} ganhou!");
                        }
                        else if (player2Choice == "0")
                        {
                            await ctx.RespondAsync($"{member.Mention} ganhou!");
                        }
                        break;
                }
            }

            async Task OnInteract(DiscordClient sender, ComponentInteractionCreateEventArgs e)
            {
                if (e.Interaction.Data.CustomId.StartsWith("rps_"))
                {
                    if (e.User.Id == ctx.User.Id)
                    {
                        if (player1Choice == null)
                        {
                            player1Choice = e.Interaction.Data.CustomId.Replace("rps_", "");
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = "Escolhido!", IsEphemeral = true });
                        }
                    }

                    if (e.User.Id == member.Id)
                    {
                        if (player2Choice == null)
                        {
                            player2Choice = e.Interaction.Data.CustomId.Replace("rps_", "");
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = "Escolhido!", IsEphemeral = true });
                        }
                    }

                    if (player1Choice != null && player2Choice != null)
                        await EndGame();
                }
                else return;
            }

            Thread.Sleep(30000);
            ctx.Client.ComponentInteractionCreated -= OnInteract;

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
        public async Task Stonks(CommandContext ctx, [RemainingText] DiscordMember member)
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
        public async Task ChangeMyMind(CommandContext ctx, params string[] args)
        {

            string text = string.Join("+", args);
            if (text.Length < 1)
                text = $"o+{ctx.User.Username}+deveria+me+dar+uma+mamada";

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.White)
            .WithImageUrl($"https://vacefron.nl/api/changemymind?text={text}");

            await ctx.RespondAsync(embed);
        }
    }
}
