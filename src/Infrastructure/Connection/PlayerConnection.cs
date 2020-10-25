using Sketch.DTOs;
using Sketch.Extensions;
using Sketch.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Connection
{
    public interface IServerConnection
    {
        void AddPlayerConnection(IPlayerConnection playerConnection);
        Task ListenTo(IEnumerable<Player> players, Action<Player, string> action);
        void RemovePlayerConnection(IPlayerConnection playerConnection);
        Task Send(ChatServerResponse serverResponse, IEnumerable<Player> players);
        void Clear();
    }

    public class ServerConnection : IServerConnection
    {
        private readonly ConcurrentDictionary<Guid, IPlayerConnection> _connections
            = new ConcurrentDictionary<Guid, IPlayerConnection>();

        public async Task Send(
            ChatServerResponse serverResponse,
            IEnumerable<Player> players)
        {
            await Task.WhenAll(players
                .Where(x => _connections.ContainsKey(x.Id))
                .Where(x => _connections[x.Id].IsConnected)
                .Select(x => _connections[x.Id].Send(serverResponse)));
        }

        public void AddPlayerConnection(IPlayerConnection playerConnection)
        {
            _connections.AddOrUpdate(
                playerConnection.Id,
                playerConnection,
                (@new, existing) => playerConnection);
        }

        public Task ListenTo(
            IEnumerable<Player> players,
            Action<Player, string> action)
        {
            var tasks = players
                .Select(x => _connections[x.Id]
                    .ReceiveString().ContinueWith(task =>
                    {
                        action(x, task.Result);
                    }));

            return Task.WhenAny(tasks);
        }

        public void RemovePlayerConnection(IPlayerConnection playerConnection)
        {
            if (!_connections.TryRemove(playerConnection.Id, out _))
            {
                throw new CannotRemovePlayerConnectionException();
            }
        }

        public void Clear()
        {
            _connections.Clear();
        }
    }

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
