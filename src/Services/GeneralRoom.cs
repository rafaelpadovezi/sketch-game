using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database;
using Sketch.Models;
using System;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public interface IGeneralRoom
    {
        string Name { get; }

        Task PlayerEnters(PlayerConnection connection);
        Task PlayerLeaves(PlayerConnection connection);
        Task PlayerSendMessage(PlayerConnection connection, string message);
    }

    public class GeneralRoom : IGeneralRoom
    {
        public string Name { get; } = "general";
        private readonly IRepository<Player> _playerRepository;
        private readonly IRepository<GameRoom> _gameRoomReposuitory;
        private readonly IServerConnection _serverConnection;

        public GeneralRoom(
            IRepository<Player> playerRepository,
            IRepository<GameRoom> gameRoomReposuitory,
            IServerConnection serverConnection)
        {
            _playerRepository = playerRepository;
            _gameRoomReposuitory = gameRoomReposuitory;
            _serverConnection = serverConnection;
        }

        public async Task PlayerEnters(PlayerConnection connection)
        {
            _serverConnection.AddPlayerConnection(connection);

            var player = await _playerRepository.GetById(connection.Id)
                ?? throw new Exception("Player not found");
            await SendAll(ChatMessage.NewPlayer(Name, player.Username));
        }

        public async Task PlayerLeaves(PlayerConnection connection)
        {
            var player = await _playerRepository.GetById(connection.Id)
                ?? throw new Exception("Player not found");
            player.IsActive = false;

            await SendAll(ChatMessage.PlayerLeftRoom(Name, player.Username));

            _serverConnection.RemovePlayerConnection(connection);
            await _playerRepository.SaveChanges();
        }

        public async Task PlayerSendMessage(PlayerConnection connection, string message)
        {
            var player = await _playerRepository.GetById(connection.Id)
                ?? throw new Exception("Player not found");

            await SendAll(ChatMessage.Public(player.Username, message));
        }

        private async Task SendAll(ChatServerResponse response)
        {
            var players = await _playerRepository
                .GetAll(x => x.IsActive && !x.GameRoomId.HasValue);

            await _serverConnection.Send(response, players);
        }
    }
}
