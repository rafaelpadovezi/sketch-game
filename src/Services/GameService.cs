using Microsoft.Extensions.Logging;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using System;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:Braces should not be omitted", Justification = "<Pending>")]
        public async Task<bool> NewCommand(PlayerConnection connection, string commandString)
        {
            if (!connection.IsConnected)
            {
                _logger.LogInformation("Player disconnected");
                await _generalRoom.PlayerLeaves(connection);
                return true;
            }

            _logger.LogInformation("Received {commandString} from ", commandString);

            var command = CommandParser.Parse(commandString);
            _logger.LogInformation("Parsed {@command}", command);

            var player = await _playerRepository.GetById(connection.Id)
                ?? throw new Exception("Player not found");

            if (command.Type == CommandType.Exit)
                await _generalRoom.PlayerLeaves(connection);
            if (command.Type == CommandType.PublicMessage)
                await _generalRoom.PlayerSendMessage(connection, command.Message);

            return command.Type == CommandType.Exit;
        }

        public async Task NewConnection(PlayerConnection connection)
        {
            await _generalRoom.PlayerEnters(connection);
        }
    }

    public interface IGameService
    {
        Task NewConnection(PlayerConnection webSocket);
        Task<bool> NewCommand(PlayerConnection webSocket, string commandString);
    }
}
