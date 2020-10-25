using Sketch.DTOs;
using Sketch.Extensions;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Connection
{
    public class PlayerConnection : IPlayerConnection
    {
        public PlayerConnection(WebSocket webSocket)
        {
            WebSocket = webSocket;
        }

        public Guid Id { get; set; }
        private WebSocket WebSocket { get; }
        public bool IsConnected =>
            WebSocket.State != WebSocketState.CloseReceived &&
            WebSocket.State != WebSocketState.Closed &&
            WebSocket.State != WebSocketState.Aborted;

        public async Task Send(ChatServerResponse serverResponse) =>
            await WebSocket.SendJsonAsync(serverResponse);

        public async Task<string> ReceiveString() =>
            await WebSocket.ReceiveStringAsync();
    }

    public interface IPlayerConnection
    {
        bool IsConnected { get; }
        Guid Id { get; set; }
        Task<string> ReceiveString();
        Task Send(ChatServerResponse serverResponse);
    }
}
