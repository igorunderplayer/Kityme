using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kityme.Entities;
using System;
using System.Threading.Tasks;
using Kityme.Extensions;
using Kityme.Managers;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Interactivity.Enums;

namespace Kityme.Commands
{
    class EconomyCommands: BaseCommandModule
    {
        [Command("saldo"), Aliases("atm", "balance", "bal")]
        public async Task Saldo(CommandContext ctx, [RemainingText] DiscordUser member = null)
        {
            member ??= ctx.User;
            User u = await member.GetAsync();
            string mine = member.Id == ctx.User.Id ? "se" : member.Username;
            await ctx.RespondAsync($"{mine} tem {u.Money} kitycoins, ta ricass neh meu");
        }


        [Command("pay"), Aliases("pagar"), Description("paga alguem")]
        public async Task Pay (CommandContext ctx, double qtd, [RemainingText] DiscordMember user)
        {
            if(double.IsNaN(qtd) || double.IsInfinity(qtd))
            {
                await ctx.RespondAsync("😳");
                return;
            }


            User author = await ctx.User.GetAsync();
            User receiver = await user.GetAsync();

            if(qtd < 1)
            {
                await ctx.RespondAsync("n me faz perder tempo \nvc perdeu 100 dinheiros");
                author.RemoveMoney(100);
                await DBManager.ReplaceUserAsync(author);
                return;
            }

            author.RemoveMoney(qtd);
            receiver.AddMoney(qtd);

            await DBManager.ReplaceUserAsync(author);
            await DBManager.ReplaceUserAsync(receiver);

            await ctx.RespondAsync($"vc pago {qtd} kitycoins com suceso!");

        }



        [Command("cats"), Description("mostra os gatos q vc ou outro usuario tem'-")]
        public async Task MyCats (CommandContext ctx, [RemainingText] DiscordUser member = null)
        {
            User user;
            if (member == null)
                user = await ctx.User.GetAsync();
            else
                user = await member.GetAsync();

            string cats = string.Empty;

            foreach(Cat cat in user.Cats)
            {
                cats += $"{cat.name} - Atratividade: {cat.atractive} \n";
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            {
                Title = "gatinhos'-",
                Description = string.IsNullOrEmpty(cats) ? "nenhum haha" : "",
                Color = DiscordColor.Gray
            };

            var interactivity = ctx.Client.GetInteractivity();
            var pages = interactivity.GeneratePagesInEmbed(cats, SplitType.Line, embedBuilder);

            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
        }

        [Command("daily"), Description("te da dinheiro de graca a cada 1 dia'-")]
        public async Task Daily (CommandContext ctx)
        {
            User user = await ctx.User.GetAsync();
            if ((DateTime.Now - user.DailyTimestamp).TotalDays < 1)
            {
                await ctx.RespondAsync($"vsf man, querendo money d graça, e nem sequer esperou pra pedir dnv'-");
                return;
            } else
            {
                double money = new Random().Next(0, 250) * user.RewardMultiplier;
                user.AddMoney(money);
                user.UpdateDailyTimestamp(DateTime.Now);
                await DBManager.ReplaceUserAsync(user);
                await ctx.RespondAsync($"palmas 👏, vc ganhou incriveis {money} kitycois totalmente de graça '-");
            }
        }

        [Command("resetme")]
        public async Task ResetMe (CommandContext ctx)
        {
            User user = await ctx.User.GetAsync();
            float totalMultiplier = 0f;
            foreach(Cat cat in user.Cats)
            {
                totalMultiplier += 0.1f;
                totalMultiplier += cat.atractive / 10;
            }
            totalMultiplier = MathF.Round(totalMultiplier, 2);
            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithContent($"voce tem certeza disso? isso ira resetar todos seus gatinhos e dinheiro (vc recebera {totalMultiplier} pontos de prestigio(o bombom la?))")
                .AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Danger, "peipei", "", false, new("✅"))
                );

            DiscordMessage msg = await ctx.RespondAsync(builder);
            var result = await msg.WaitForButtonAsync();

            if (result.TimedOut)
            {
                await msg.DeleteAsync();
            } else
            {
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                User newUser = new User(user.ID);
                newUser.RewardMultiplier = user.RewardMultiplier + totalMultiplier;
                await DBManager.ReplaceUserAsync(newUser);
                await ctx.RespondAsync("vc foi resetado com sucesso 🗿");
            }
            
        }

        [Command("buy"), Aliases("comprar"), Description("compra algo ora'-")]
        public async Task Buy (CommandContext ctx, string item = null)
        {
            if(item == null)
            {
                await ctx.RespondAsync("insira seu pe... algo para compra'-");
                return;
            }

            User user = await ctx.User.GetAsync();
            if(user.Money < 1500f)
            {
                await ctx.RespondAsync("vc n tem dinheiros suficiente pra compra'-");
                return;
            }
            Random rand = new();
            int atr = rand.Next(1, 10);
            Cat cat = new Cat(atr, "Cat");
            cat.name = cat.type;

            int code = rand.Next(0, 1000);
            if(code == 13)
            {
                cat.atractive = 10000;
                cat.name = $"Gato {cat.type} Supremo";
                user.Cats.Add(cat);
                user.RemoveMoney(1500f);
                await DBManager.ReplaceUserAsync(user);
                
                return;
            }

            user.Cats.Add(cat);
            user.RemoveMoney(1500f);
            await DBManager.ReplaceUserAsync(user);
            await ctx.RespondAsync($"parabens, agr vc tem um Gato {cat.type} '-");
        }

        [Command("buy")]
        public async Task Buy(CommandContext ctx,  string item = null, uint qtd = 1)
        {
            if (item == null)
            {
                await ctx.RespondAsync("insira seu pe... algo para compra'-");
                return;
            }

            User user = await ctx.User.GetAsync();
            if (user.Money < 1500f * qtd)
            {
                await ctx.RespondAsync("vc n tem dinheiros suficiente pra compra'-");
                return;
            }
            int boughts = 0;
            Random rand = new Random();
            StringBuilder builder = new StringBuilder();
            while (boughts < qtd)
            {
                int atr = rand.Next(1, 10);
                Cat cat = new Cat(atr, "Cat");
                cat.name = cat.type;

                int code = rand.Next(0, 1000);
                if (code == 13)
                {
                    cat.atractive = 10000;
                    cat.name = $"Gato {cat.type} Supremo";
                    builder.AppendLine($"parabens agor...... forças divinas se aproximam.. agora voce tem um {cat.name} :D");
                }

                user.Cats.Add(cat);
                boughts++;
            }
            
            user.RemoveMoney(1500f * qtd);
            await DBManager.ReplaceUserAsync(user);
            builder.AppendLine($"parabens, agr vc compro {qtd} '-");
            await ctx.RespondAsync(builder.ToString());
        }

        [Command("catshow")]
        public async Task CatShow (CommandContext ctx)
        {
            User user = await ctx.User.GetAsync();

            if((DateTime.Now - user.ShowTimestamp).TotalDays < 7)
            {
                await ctx.RespondAsync("vc n pode faze um show agr, espera mais ae'-");
                return;
            } else
            {
                int total = 0;
                foreach(Cat cat in user.Cats)
                {
                    total += cat.atractive;
                }
                double money = total * 100 * user.RewardMultiplier;
                user.AddMoney(money);
                user.ShowTimestamp = DateTime.Now;
                await DBManager.ReplaceUserAsync(user);

                await ctx.RespondAsync($"seu show rendeu {money} kitycois e teve {money / 5} participantes");
            }
        }

        [Command("ytcatvideo")]
        public async Task YTCatVideo(CommandContext ctx)
        {
            User user = await ctx.User.GetAsync();

            if ((DateTime.Now - user.ShowTimestamp).TotalHours < 12)
            {
                await ctx.RespondAsync("vc so pode grava/posta um video a cada 12 horas");
                return;
            }
            else
            {
                int total = 0;
                foreach (Cat cat in user.Cats)
                {
                    total += cat.atractive;
                }
                double money = total * 25 * user.RewardMultiplier;
                user.AddMoney(money);
                user.YTVideoTimestamp = DateTime.Now;
                await DBManager.ReplaceUserAsync(user);

                await ctx.RespondAsync($"seu video te rendeu {money} kitycois e teve {money * 3} views");
            }
        }
    }
}
