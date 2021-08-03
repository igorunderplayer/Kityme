using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kityme.Commands
{
    class RPCommands: BaseCommandModule
    {
        [Command("hug")]
        async Task Hug (CommandContext ctx, [Description("membro q vc vai abraça")]DiscordMember member)
        {
            await ctx.Channel.SendMessageAsync($"{ctx.User.Mention} abraçou {member.Username} '-");
        }
    }
}
