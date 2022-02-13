using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Kityme.Managers
{
    static class MusicManagers
    {
        public static Dictionary<ulong, GuildMusicManager> _managers = new();

        public static async Task UpdatePack (DiscordClient client, DiscordVoiceState oldState, DiscordVoiceState newState)
        {

            if(_managers.ContainsKey(newState.Guild.Id))
            {
                GuildMusicManager manager = _managers[newState.Guild.Id];
                if (newState == null || newState.Channel == null)
                {
                    await manager.Connection.DisconnectAsync();
                    _managers.Remove(manager.Connection.Guild.Id);
                    return;
                }

                if (newState.Channel != oldState.Channel)
                {
                    var newManager = new GuildMusicManager(manager.Client, newState.Guild, newState.Channel, (id) => _managers.Remove(id));
                    newManager.ActualIndex = manager.ActualIndex;
                    newManager.LastMessage = manager.LastMessage;
                    newManager._queue = manager._queue;
                    newManager._loopEnabled = manager._loopEnabled;
                    newManager._shuffleEnabled = manager._shuffleEnabled;

                    await newManager.Connection.PlayPartialAsync(newManager._queue[newManager.ActualIndex], manager.Connection.CurrentState.PlaybackPosition, newManager._queue[newManager.ActualIndex].Length);
                    manager.Connection.PlaybackFinished -= manager.Connection_PlaybackFinished;

                    _managers.Remove(oldState.Guild.Id);
                    _managers.Add(newManager.Connection.Guild.Id, newManager);
                }

                // } else if (newState.Channel.Id != manager.Connection?.Channel?.Id)
                // {
                //     if(manager.Connection != null)
                //         await manager.Connection.DisconnectAsync();
                //     manager.Connection = await manager.Connection?.Node.ConnectAsync(newState.Channel);
                //     await manager.Connection.PlayAsync(manager._queue[manager.ActualIndex]);
                // }
            }
            return;
        }
    }
}