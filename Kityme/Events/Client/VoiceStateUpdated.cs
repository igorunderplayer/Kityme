using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Kityme.Managers;

namespace Kityme.Events.Client
{
    public class VoiceStateUpdated
    {
        private readonly DiscordClient _client;
        public VoiceStateUpdated(DiscordClient client)
        {
            this._client = client;
        }

        public Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if(e.Before?.User.Id == client.CurrentUser.Id)
            {
                MusicManagers.UpdatePack(client, e.Before, e.After);
            }
            return Task.CompletedTask;
        }
    }
}