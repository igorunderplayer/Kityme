using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Lavalink;
using DSharpPlus.Net.WebSocket;

namespace  Kityme.Extensions
{
    public static class LavalinkNodeConnectionExtension
    {
        public static async Task SendAsync (this LavalinkNodeConnection node, string message)
        {
            WebSocketClient ws = node.GetType().GetProperty("WebSocket", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(node) as WebSocketClient;
            await ws.SendMessageAsync(message);
        }
    }
}