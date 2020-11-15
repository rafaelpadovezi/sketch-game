using AutoMapper;
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
        private readonly IGeneralRoomService _generalRoomService;
        private readonly IGameRoomService _gameRoomService;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IServerConnection _server;
        private readonly IMapper _mapper;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IGeneralRoomService generalRoomService,
            IGameRoomService gameRoomService,
            IPlayerRepository playerRepository,
            IGameRoomRepository gameRoomRepository,
            IServerConnection server,
            IMapper mapper,
            ILogger<GameService> logger)
        {
            _generalRoomService = generalRoomService;
            _gameRoomService = gameRoomService;
            _playerRepository = playerRepository;
            _gameRoomRepository = gameRoomRepository;
            _server = server;
            _mapper = mapper;
            _logger = logger;
        }

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "<Pending>")]
        public async Task<bool> NewCommand(Guid playerId, string commandString)
        {
            _logger.LogInformation("Received {commandString} from ", commandString);

            var command = CommandParser.Parse(commandString);
            _logger.LogInformation("Parsed {@command}", command);
            var player = await _playerRepository.GetById(playerId)
                    ?? throw new Exception("Player not found");

            var commandTask = command.Type switch
            {
                CommandType.PublicMessage  => SendMessage(playerId, commandString, command, player),
                CommandType.ChangeGameRoom => ChangeGameRoom(playerId, command, player),
                CommandType.ListChatRooms  => ListGameRooms(player),
                _ => Task.CompletedTask
            };

            await commandTask;

            return command.Type == CommandType.Exit;
        }

        private async Task ListGameRooms(Models.Player player)
        {
            var gameRooms = (await _gameRoomRepository.GetAll(_ => true))
                .Select(x => _mapper.Map<GameRoomViewModel>(x));
            await _server.Send(ChatServerResponse.ListChatRooms(gameRooms), player);
        }

        private async Task ChangeGameRoom(Guid playerId, ChatCommand command, Models.Player player)
        {
            if (player.GameRoomId.HasValue && command.GameRoomName == _generalRoomService.Name)
            {
                await _gameRoomService.LeaveGameRoom(player, _generalRoomService.Name);
                await _generalRoomService.Enter(playerId);
            }
            else
            {
                await _gameRoomService.EnterGameRoom(command.GameRoomName, player);
                await _generalRoomService.LeaveToGameRoom(player, command.GameRoomName);
            }
        }

        private async Task SendMessage(Guid playerId, string commandString, ChatCommand command, Models.Player player)
        {
            if (player.GameRoomId.HasValue)
            {
                await _gameRoomService.SendMessage(commandString, player);
            }
            else
            {
                await _generalRoomService.PlayerSendMessage(playerId, command.Message);
            }
        }

        public async Task NewPlayer(Guid playerId)
        {
            await _generalRoomService.Enter(playerId);
        }

        public async Task PlayerLeaves(Guid playerId)
        {
            await _generalRoomService.PlayerLeaves(playerId);
        }
    }
}
