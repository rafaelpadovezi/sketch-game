using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sketch.Extensions
{
    public static class WebSocketExtensions
    {
        public static async Task SendJsonAsync<T>(this WebSocket webSocket,
            T result, CancellationToken cancelationToken = default)
        {
            string jsonMessage = JsonSerializer.Serialize(result);
            await webSocket.SendStringAsync(jsonMessage, cancelationToken);
        }

        public static async Task SendStringAsync(this WebSocket webSocket,
            string text, CancellationToken cancelationToken = default)
        {
            await webSocket.SendAsync(buffer: new ArraySegment<byte>(
                array: Encoding.UTF8.GetBytes(text),
                offset: 0,
                count: text.Length),
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: cancelationToken);
        }

        public static async Task<T> ReceiveJsonAsync<T>(
            this WebSocket websSocket)
        {
            var message = await websSocket.ReceiveStringAsync();
            return JsonSerializer.Deserialize<T>(message);
        }

        public static async Task<string> ReceiveStringAsync(
            this WebSocket websSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await websSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            StringBuilder message = new StringBuilder();
            while (!result.EndOfMessage)
            {
                result = await websSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
                message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }

            message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            return message.ToString();
        }
    }
}
