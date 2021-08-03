using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kityme.Entities;
using Kityme.Extensions;
using Kityme.Managers;

namespace Kityme.Commands
{
    class DevCommands: BaseCommandModule
    {
        [Command("addmoney"), RequireOwner]
        public async Task AddMoney (CommandContext ctx, double qtd, [RemainingText] DiscordMember member = null)
        {
            if (member == null)
                member = ctx.Member;

            User u = await member.GetAsync();
            u.AddMoney(qtd);
            await DBManager.ReplaceUserAsync(u);
            await ctx.RespondAsync($"doei {qtd} kitycois para vc");
        }

        [Command("removemoney"), RequireOwner]
        public async Task RemoveMoney(CommandContext ctx, double qtd, [RemainingText] DiscordMember member = null)
        {
            if (member == null)
                member = ctx.Member;

            User u = await member.GetAsync();
            u.RemoveMoney(qtd);
            await DBManager.ReplaceUserAsync(u);
            await ctx.RespondAsync($"removi {qtd} kitycois para vc");
        }

        [Command("resetuser"), RequireOwner]
        public async Task ResetUser (CommandContext ctx, [RemainingText] DiscordMember member = null)
        {
            if (member == null)
                member = ctx.Member;

            User u = new User(member.Id);
            await DBManager.ReplaceUserAsync(u);

            await ctx.RespondAsync("resetei pra vc'-");
        }

    }
}
