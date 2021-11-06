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

            if(qtd > 1)
            {
                await ctx.RespondAsync("n me faz perder tempo \nvc perdeu 100 dinheiros");
                author.RemoveMoney(100);
                await DBManager.ReplaceUserAsync(author);
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
            User user = null;
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
                Description = string.IsNullOrEmpty(cats) ? "nenhum haha" : cats,
                Color = DiscordColor.Gray
            };

            await ctx.RespondAsync(embedBuilder);
        }

        [Command("daily"), Description("te da dinheiro de graca a cada 1 dia'-")]
        public async Task Daily (CommandContext ctx)
        {
            User user = await ctx.User.GetAsync();
            if ((DateTime.Now - user.DailyTimestamp).Days < 1)
            {
                await ctx.RespondAsync($"vsf man, querendo money d graça, e nem sequer esperou pra pedir dnv'-");
                return;
            } else
            {
                double money = new Random().Next(0, 250);
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
            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithContent($"voce tem certeza disso? isso ira resetar todos seus gatinhos e dinheiro (vc recebera {totalMultiplier} pontos de prestigio(o bombom la?))")
                .AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Danger, "peipei", "", false, new("✅"))
                );

            DiscordMessage msg = await ctx.RespondAsync(builder);
            var inte = ctx.Client.GetInteractivity();
            var result = await msg.WaitForButtonAsync();

            if (result.TimedOut)
            {
                await msg.DeleteAsync();
            } else
            {
                var newUser = new User(user.ID);
                newUser.RewardMultiplier = user.RewardMultiplier + totalMultiplier;
                await DBManager.ReplaceUserAsync(newUser);
                await ctx.RespondAsync("vc foi resetado com sucesso 🗿");
            }
            
        }

        [Command("buy"), Aliases("comprar"), Description("compra algo ora'-")]
        public async Task Buy (CommandContext ctx, [RemainingText] string item = null)
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

            user.Cats.Add(cat);
            user.RemoveMoney(1500f);
            await DBManager.ReplaceUserAsync(user);
            await ctx.RespondAsync($"parabens, agr vc tem um Gato {cat.type} '-");
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
                double money = total * 100;
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
                double money = total * 25;
                user.AddMoney(money);
                user.YTVideoTimestamp = DateTime.Now;
                await DBManager.ReplaceUserAsync(user);

                await ctx.RespondAsync($"seu video te rendeu {money} kitycois e teve {money * 3} views");
            }
        }
    }
}
