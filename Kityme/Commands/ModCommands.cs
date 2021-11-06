using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Kityme.Commands
{
    class ModCommands: BaseCommandModule 
    {
        [Command("clear"),
            Description("apaga as mensagens do chat tlg?"),
            Aliases("clean", "limpar"),
            RequirePermissions(Permissions.ManageMessages),
            RequireBotPermissions(Permissions.ManageMessages)]
        public async Task Clear (CommandContext ctx, ushort count)
        {
            var messages = await ctx.Channel.GetMessagesAsync(count + 1);
            await ctx.Channel.DeleteMessagesAsync(messages);
            await ctx.RespondAsync($"{count} mensagens apagadas por {ctx.User.Mention}");
        }

        [Command("ban"),
            Description("bane algum"),
            Aliases("banir"),
            RequirePermissions(Permissions.BanMembers),
            RequireBotPermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "")
        {
            if(member == null)
            {
                await ctx.RespondAsync("tu ta ligado q precisa escolhe um membro pra banir ne?");
                return;
            }

            if(member.Hierarchy > ctx.Member.Hierarchy)
            {
                await ctx.RespondAsync("num podi");
                return;
            }

            await member.BanAsync(3, reason);
            await ctx.RespondAsync("banidor");
        }
    }
}
