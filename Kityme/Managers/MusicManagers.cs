using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Kityme.Managers
{
    static class MusicManagers
    {
        public static Dictionary<ulong, GuildMusicManager> _managers = new();

        public static async Task UpdatePack (DiscordClient client, DiscordVoiceState voiceState)
        {

            if(_managers.ContainsKey(voiceState.Guild.Id))
            {
                GuildMusicManager manager = _managers[voiceState.Guild.Id];
                if (voiceState == null || voiceState.Channel == null)
                {
                    await manager.Connection.DisconnectAsync();
                    _managers.Remove(manager.Connection.Guild.Id);

                } else if (voiceState.Channel.Id != manager.Connection?.Channel?.Id)
                {
                    if(manager.Connection != null)
                        await manager.Connection.DisconnectAsync();
                    manager.Connection = await manager.Connection?.Node.ConnectAsync(voiceState.Channel);
                    await manager.Connection.PlayAsync(manager._queue[manager.actualIndex]);
                }
            }
            return;
        }
    }
}