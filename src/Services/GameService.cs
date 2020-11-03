using Microsoft.Extensions.Logging;
using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class GameService : IGameService
    {
        private readonly IGeneralRoom _generalRoom;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IServerConnection _server;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IGeneralRoom generalRoom,
            IPlayerRepository playerRepository,
            IGameRoomRepository gameRoomRepository,
            IServerConnection server,
            ILogger<GameService> logger)
        {
            _generalRoom = generalRoom;
            _playerRepository = playerRepository;
            _gameRoomRepository = gameRoomRepository;
            _server = server;
            _logger = logger;
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:Braces should not be omitted", Justification = "<Pending>")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1508:Closing braces should not be preceded by blank line", Justification = "<Pending>")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:Closing brace should be followed by blank line", Justification = "<Pending>")]
        public async Task<bool> NewCommand(Guid playerId, string commandString)
        {
            _logger.LogInformation("Received {commandString} from ", commandString);

            var command = CommandParser.Parse(commandString);
            _logger.LogInformation("Parsed {@command}", command);
            var player = await _playerRepository.GetById(playerId)
                    ?? throw new Exception("Player not found");

            if (command.Type == CommandType.PublicMessage)
            {
                if (player.GameRoomId.HasValue)
                {
                    await SendGameRoomMessage(commandString, player);
                }
                else
                {
                    await _generalRoom.PlayerSendMessage(playerId, command.Message);
                }
            }
            if (command.Type == CommandType.ChangeGameRoom)
            {
                if (player.GameRoomId.HasValue && command.GameRoomName == "general")
                {
                    await _generalRoom.PlayerEnters(playerId);
                }
                else
                {
                    await ChangeGameRoom(command, player);
                }
            }
            if (command.Type == CommandType.ListChatRooms)
            {
                var gameRooms = (await _gameRoomRepository.GetAll(_ => true)).Select(x => x.Name);
                await _server.Send(ChatServerResponse.ListChatRooms(gameRooms), player);
            }

            return command.Type == CommandType.Exit;
        }

        private async Task ChangeGameRoom(ChatCommand command, Models.Player player)
        {
            var gameroom = await _gameRoomRepository.Get(x => x.Name == command.GameRoomName)
                ?? throw new Exception($"GameRoom '{command.GameRoomName}' not found");
            player.GameRoomId = gameroom.Id;
            gameroom.Players.Add(player);
            await _generalRoom.PlayerEntersGameRoom(player, gameroom);
            await _gameRoomRepository.SaveChanges();
            await _server.Send(ChatServerResponse.EnterGameRoom(gameroom.Name), player);
            await SendGameRoomMessage($"\"{player.Username} has joined #{gameroom.Name}\"", player, gameroom);
        }

        private async Task SendGameRoomMessage(string message, Models.Player player)
        {
            var gameRoom = await _gameRoomRepository.Get(x => x.Id == player.GameRoomId)
                ?? throw new Exception($"GameRoom '{player.GameRoomId}' not found");
            await SendGameRoomMessage(message, player, gameRoom);
        }

        private async Task SendGameRoomMessage(string message, Models.Player player, Models.GameRoom gameRoom)
        {
            var gameRoomPlayers = await _playerRepository.GetAll(x => x.GameRoomId == gameRoom.Id);
            await _server.Send(ChatMessage.Public(player.Username, message), gameRoomPlayers);
        }

        public async Task NewPlayer(Guid playerId)
        {
            await _generalRoom.PlayerEnters(playerId);
        }

        public async Task PlayerLeaves(Guid playerId)
        {
            await _generalRoom.PlayerLeaves(playerId);
        }
    }

    public interface IGameService
    {
        Task NewPlayer(Guid playerId);
        Task<bool> NewCommand(Guid playerId, string commandString);
        Task PlayerLeaves(Guid playerId);
    }
}
