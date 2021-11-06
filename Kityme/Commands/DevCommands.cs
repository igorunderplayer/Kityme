using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kityme.Entities;
using Kityme.Extensions;
using Kityme.Managers;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Kityme.Commands
{
    class DevCommands: BaseCommandModule
    {
        [Command("eval"), Aliases("evalcs", "cseval", "roslyn"), Description("Evaluates C# code."), Hidden, RequireOwner]
        public async Task EvalCS(CommandContext ctx, [RemainingText] string code)
        {
            var msg = ctx.Message;

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.");

            var cs = code.Substring(cs1, cs2 - cs1);

            msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Evaluating...")
                .Build()).ConfigureAwait(false);

            try
            {
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx);

                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity", "Microsoft.Extensions.Logging");
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                var result = await script.RunAsync(globals).ConfigureAwait(false);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Result", Description = result.ReturnValue.ToString(), Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
                else
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Successful", Description = "No result was returned.", Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color = new DiscordColor("#FF0000") }.Build()).ConfigureAwait(false);
            }
        }

        [Command("gc"), RequireOwner]
        public async Task GC (CommandContext ctx)
        {
            System.GC.Collect();
            await ctx.RespondAsync("e");
        }


        [Command("showguilds"), RequireOwner]
        public async Task ShowGuilds (CommandContext ctx) 
        {
            string guilds = string.Empty;
            foreach(DiscordGuild guild in ctx.Client.Guilds.Values) 
            {
                guilds += $"{guild.Name} \n";
            }
            
            await ctx.RespondAsync(guilds);
        }
    
        [Command("addmoney"), RequireOwner]
        public async Task AddMoney (CommandContext ctx, double qtd, [RemainingText] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            User u = await user.GetAsync();
            u.AddMoney(qtd);
            await DBManager.ReplaceUserAsync(u);
            await ctx.RespondAsync($"doei {qtd} kitycois para vc");
        }

        [Command("removemoney"), RequireOwner]
        public async Task RemoveMoney(CommandContext ctx, double qtd, [RemainingText] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            User u = await user.GetAsync();
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

    public class TestVariables
    {
        public DiscordMessage Message { get; set; }
        public DiscordChannel Channel { get; set; }
        public DiscordGuild Guild { get; set; }
        public DiscordUser User { get; set; }
        public DiscordMember Member { get; set; }
        public CommandContext ctx { get; set; }

        public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext _ctx)
        {
            this.Client = client;

            this.Message = msg;
            this.Channel = msg.Channel;
            this.Guild = this.Channel.Guild;
            this.User = this.Message.Author;
            if (this.Guild != null)
                this.Member = this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            this.ctx = _ctx;
        }

        public DiscordClient Client;
    }
}
