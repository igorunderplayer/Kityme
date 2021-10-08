using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;


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
    }
}
