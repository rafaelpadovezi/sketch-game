using Microsoft.Extensions.Logging;
using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Models;
using System;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class GameRoomService : IGameRoomService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IServerConnection _server;
        private readonly IRoundService _roundService;
        private readonly ILogger<GameRoomService> _logger;

        public GameRoomService(
            IPlayerRepository playerRepository,
            IGameRoomRepository gameRoomRepository,
            IServerConnection server,
            IRoundService roundService,
            ILogger<GameRoomService> logger)
        {
            _playerRepository = playerRepository;
            _gameRoomRepository = gameRoomRepository;
            _server = server;
            _roundService = roundService;
            _logger = logger;
        }

        public async Task EnterGameRoom(string gameRoomName, Models.Player player)
        {
            var gameroom = await _gameRoomRepository.Get(x => x.Name == gameRoomName)
                ?? throw new Exception($"GameRoom '{gameRoomName}' not found");
            player.GameRoomId = gameroom.Id;
            gameroom.Players.Add(player);

            await _gameRoomRepository.SaveChanges();

            await _server.Send(ChatServerResponse.EnterGameRoom(gameroom.Name), player);
            await SendGameRoomMessage(ChatMessage.NewPlayer(gameroom.Name, player.Username), player, gameroom);

            if (gameroom.Players.Count == 2)
            {
                await _roundService.StartRound(gameroom);
                return;
            }

            var turn = gameroom.CurrentTurn();
            if (turn is not null)
            {
                await _roundService.AddToTurn(gameroom, turn, player);
                await _gameRoomRepository.SaveChanges();
            }
        }

        public async Task LeaveGameRoom(Player player, string newGameRoom)
        {
            var gameRoomId = player.GameRoomId.HasValue
                ? player.GameRoomId.Value
                : throw new Exception("Player ins't in a room");
            var gameRoom = (await _gameRoomRepository.GetById(gameRoomId))
                ?? throw new Exception($"GameRoom '{player.GameRoomId}' not found");

            await SendGameRoomMessage(ChatMessage.ChangeRoom(player.Username, newGameRoom), player, gameRoom);
            await _server.Send(ChatServerResponse.EnterGameRoom(newGameRoom), player);
            await RemovePlayer(gameRoom, player);
        }

        public async Task Leave(Player player)
        {
            var gameRoomId = player.GameRoomId.HasValue
                ? player.GameRoomId.Value
                : throw new Exception("Player ins't in a room");
            var gameRoom = (await _gameRoomRepository.GetById(gameRoomId))
                ?? throw new Exception($"GameRoom '{player.GameRoomId}' not found");

            await SendGameRoomMessage(ChatMessage.LeftGame(player.Username), player, gameRoom);
            await RemovePlayer(gameRoom, player);
        }

        private async Task RemovePlayer(GameRoom gameRoom, Player player)
        {
            gameRoom.Players.Remove(player);
            if (gameRoom.Players.Count == 1)
                await _roundService.EndTurn(gameRoom);

            await _roundService.PlayerLeavesTurn(gameRoom, player);
            await _gameRoomRepository.SaveChanges();
        }

        public async Task GuessOrSendMessage(string message, Player player)
        {
            if (!player.GameRoomId.HasValue)
            {
                _logger.LogWarning("Player {playerId} has no gameRommId", player.Id);
                return;
            }

            var gameRoom = await _gameRoomRepository.Get(x => x.Id == player.GameRoomId)
                ?? throw new Exception($"GameRoom '{player.GameRoomId}' not found");
            var turn = gameRoom.CurrentTurn();
            if (turn is null)
            {
                await _server.Send(ChatMessage.Public(player.Username, message), gameRoom.Players);
                return;
            }

            await _roundService.GuessWord(gameRoom, player, turn, message);
            await _gameRoomRepository.SaveChanges();
        }

        private async Task SendGameRoomMessage(ChatMessage message, Models.Player player, Models.GameRoom gameRoom)
        {
            var gameRoomPlayers = await _playerRepository.GetAll(x => x.GameRoomId == gameRoom.Id);
            await _server.Send(message, gameRoomPlayers);
        }
    }
}
