using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Kityme.Extensions
{
    public static class LavalinkTrackExtension
    {
        private static DiscordMember requester;

        public static void SetRequester(this LavalinkTrack track, DiscordMember member)
            => requester = member;

        public static DiscordMember GetRequester(this LavalinkTrack track)
            => requester;
    }
}