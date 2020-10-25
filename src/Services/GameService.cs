using Microsoft.Extensions.Logging;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class GameService : IGameService
    {
        private readonly IGeneralRoom _generalRoom;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IGeneralRoom generalRoom,
            IPlayerRepository playerRepository,
            ILogger<GameService> logger)
        {
            _generalRoom = generalRoom;
            _playerRepository = playerRepository;
            _logger = logger;
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:Braces should not be omitted", Justification = "<Pending>")]
        public async Task<bool> NewCommand(Guid playerId, string commandString)
        {
            _logger.LogInformation("Received {commandString} from ", commandString);

            var command = CommandParser.Parse(commandString);
            _logger.LogInformation("Parsed {@command}", command);

            var player = await _playerRepository.GetById(playerId)
                ?? throw new Exception("Player not found");

            if (command.Type == CommandType.Exit)
                await _generalRoom.PlayerLeaves(playerId);
            if (command.Type == CommandType.PublicMessage)
                await _generalRoom.PlayerSendMessage(playerId, command.Message);

            return command.Type == CommandType.Exit;
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
