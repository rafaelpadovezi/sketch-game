using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database;
using Sketch.Models;
using System;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public interface IGeneralRoomService
    {
        string Name { get; }
        Task Enter(Guid playerId);
        Task PlayerLeaves(Guid playerId);
        Task PlayerSendMessage(Guid playerId, string message);
        Task LeaveToGameRoom(Player player, string gameroom);
    }

    public class GeneralRoomService : IGeneralRoomService
    {
        public string Name { get; } = "general";
        private readonly IRepository<Player> _playerRepository;
        private readonly IServerConnection _serverConnection;

        public GeneralRoomService(
            IRepository<Player> playerRepository,
            IServerConnection serverConnection)
        {
            _playerRepository = playerRepository;
            _serverConnection = serverConnection;
        }

        public async Task Enter(Guid playerId)
        {
            var player = await _playerRepository.GetById(playerId)
                ?? throw new Exception("Player not found");
            player.GameRoomId = null;
            await _playerRepository.SaveChanges();

            await SendAll(ChatMessage.NewPlayer(Name, player.Username));
        }

        public async Task PlayerLeaves(Guid playerId)
        {
            var player = await _playerRepository.GetById(playerId)
                ?? throw new Exception("Player not found");
            player.IsActive = false;

            await SendAll(ChatMessage.PlayerLeftRoom(Name, player.Username));

            await _playerRepository.SaveChanges();
        }

        public async Task PlayerSendMessage(Guid playerId, string message)
        {
            var player = await _playerRepository.GetById(playerId)
                ?? throw new Exception("Player not found");

            await SendAll(ChatMessage.Public(player.Username, message));
        }

        private async Task SendAll(ChatServerResponse response)
        {
            var players = await _playerRepository
                .GetAll(x => x.IsActive && !x.GameRoomId.HasValue);

            await _serverConnection.Send(response, players);
        }

        public async Task LeaveToGameRoom(Player player, string gameroom)
        {
            await SendAll(ChatMessage.ChangeRoom(player.Username, gameroom));
        }
    }
}
