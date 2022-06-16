using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kityme.Entities;
using System;
using System.Threading.Tasks;
using Kityme.Extensions;
using Kityme.Managers;

namespace Kityme.Commands
{
    class EconomyCommands: BaseCommandModule
    {
        [Command("money"), Aliases("saldo", "atm", "balance", "bal")]
        public async Task Saldo(CommandContext ctx, [RemainingText] DiscordUser member = null)
        {
            await ctx.RespondAsync("em construção");
            member ??= ctx.User;
            User u = await member.GetAsync();
            string mine = member.Id == ctx.User.Id ? "se" : member.Username;
            await ctx.RespondAsync($"{mine} tem {u.Money} kitycoins, ta ricass neh meu");
        }

        [Command("profile"), Description("mostra algumas infos sobre um tal user")]
        public async Task Profile (CommandContext ctx, [RemainingText] DiscordUser user = null)
        {
            await ctx.RespondAsync("em construção");
            user ??= ctx.User;
            User dbUser = await user.GetAsync();

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithAuthor(user.Username, iconUrl: user.AvatarUrl)
                .WithTitle($"Profile de {user.Username}#{user.Discriminator}")
                .WithDescription($"pontos de prestigio: {dbUser.GlobalRewardMultiplier}")
                .WithColor(DiscordColor.PhthaloBlue);

            await ctx.RespondAsync(embedBuilder);
        }

        [Command("pay"), Aliases("pagar"), Description("paga alguem")]
        public async Task Pay (CommandContext ctx, double qtd, [RemainingText] DiscordMember user)
        {
            await ctx.RespondAsync("em construção");

            if (double.IsNaN(qtd) || double.IsInfinity(qtd))
            {
                await ctx.RespondAsync("😳");
                return;
            }

            User author = await ctx.User.GetAsync();
            User receiver = await user.GetAsync();
            qtd = Math.Round(qtd, 2);

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

        [Command("daily"), Description("te da dinheiro de graca a cada 1 dia'-")]
        public async Task Daily (CommandContext ctx)
        {
            await ctx.RespondAsync("em construção");
            User user = await ctx.User.GetAsync();
            if ((DateTime.UtcNow - user.DailyTimestamp).TotalDays < 1)
            {
                await ctx.RespondAsync($"vsf man, querendo money d graça, e nem sequer esperou pra pedir dnv'-");
                return;
            } else
            {
                double money = new Random().Next(5, 250) * user.GlobalRewardMultiplier;
                if (DateTime.UtcNow.Day == 25 && DateTime.UtcNow.Month == 12)
                {
                    money *= 5;
                    await ctx.RespondAsync("feliz natal");
                }
                money = Math.Round(money, 2);
                user.AddMoney(money);
                user.UpdateDailyTimestamp(DateTime.UtcNow);
                await DBManager.ReplaceUserAsync(user);
                await ctx.RespondAsync($"palmas 👏, vc ganhou incriveis {money} kitycois totalmente de graça '-");
            }
        }

        [Command("buy"), Aliases("comprar"), Description("compra algo ora'-")]
        public async Task Buy(CommandContext ctx,  string item = null, uint qtd = 1)
        {
            await ctx.RespondAsync("em construção");
        }
    }
}
