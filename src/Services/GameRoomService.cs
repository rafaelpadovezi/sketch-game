using AutoMapper;
using Microsoft.Extensions.Logging;
using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class GameRoomService : IGameRoomService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IServerConnection _server;
        private readonly IMapper _mapper;
        private readonly ILogger<GameRoomService> _logger;

        public GameRoomService(
            IPlayerRepository playerRepository,
            IGameRoomRepository gameRoomRepository,
            IServerConnection server,
            IMapper mapper,
            ILogger<GameRoomService> logger)
        {
            _playerRepository = playerRepository;
            _gameRoomRepository = gameRoomRepository;
            _server = server;
            _mapper = mapper;
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
        }

        public async Task LeaveGameRoom(Models.Player player, string newGameRoom)
        {
            var gameRoomId = player.GameRoomId.HasValue
                ? player.GameRoomId.Value
                : throw new Exception("Player ins't in a room");

            var gameRoom = (await _gameRoomRepository.GetById(gameRoomId))
                ?? throw new Exception($"GameRoom '{player.GameRoomId}' not found");
            await SendGameRoomMessage(ChatMessage.ChangeRoom(player.Username, newGameRoom), player, gameRoom);
            await _server.Send(ChatServerResponse.EnterGameRoom(newGameRoom), player);
        }

        public async Task SendMessage(string message, Models.Player player)
        {
            var gameRoom = await _gameRoomRepository.Get(x => x.Id == player.GameRoomId)
                ?? throw new Exception($"GameRoom '{player.GameRoomId}' not found");
            await SendGameRoomMessage(ChatMessage.Public(player.Username, message), player, gameRoom);
        }

        private async Task SendGameRoomMessage(ChatMessage message, Models.Player player, Models.GameRoom gameRoom)
        {
            var gameRoomPlayers = await _playerRepository.GetAll(x => x.GameRoomId == gameRoom.Id);
            await _server.Send(message, gameRoomPlayers);
        }
    }
}
