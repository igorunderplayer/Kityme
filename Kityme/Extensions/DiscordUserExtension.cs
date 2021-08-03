using System.Threading.Tasks;
using DSharpPlus.Entities;
using Kityme.Entities;
using static Kityme.Managers.DBManager;

namespace Kityme.Extensions
{
    public static class DiscordUserExtension
    {
        public static async Task<User> GetAsync(this DiscordUser user)
            => await GetUserAsync(user.Id);

        public static async Task RegistUserAsync(this DiscordUser user)
            => await CreateUserAsync(new User(user.Id));
    }
}