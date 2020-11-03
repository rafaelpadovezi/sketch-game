using Infrastructure.Database;
using Moq;
using Sketch.Infrastructure.Connection;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Support
{
    internal class TestingScenarioBuilder
    {
        private readonly SketchDbContext _context;

        public TestingScenarioBuilder(SketchDbContext context)
        {
            _context = context;
        }

        public async Task<Player> BuildScenarioWithPlayer(string username)
        {
            Player player = new Player { Username = username };
            _context.Add(player);

            await _context.SaveChangesAsync();

            return player;
        }

        public async Task<IList<Mock<IPlayerConnection>>> BuildScenarioWith3ConnectedPlayers(
            IServerConnection server)
        {
            var players = new List<Player>()
            {
                new Player { Username = "Player1" },
                new Player { Username = "Player2" },
                new Player { Username = "Player3" }
            };

            _context.AddRange(players);

            await _context.SaveChangesAsync();

            var mocks = players.Select(x => CreateMockPlayer(x)).ToList();
            foreach (var mock in mocks)
            {
                server.AddPlayerConnection(mock.Object);
            }

            return mocks;
        }

        private static Mock<IPlayerConnection> CreateMockPlayer(Player player)
        {
            var mock = new Mock<IPlayerConnection>();
            mock.Setup(m => m.Id).Returns(player.Id);
            mock.Setup(m => m.IsConnected).Returns(true);
            return mock;
        }

        public async Task<IList<Mock<IPlayerConnection>>> BuildScenarioWith3PlayersAndGameRoom(
            IServerConnection server)
        {
            var players = new List<Player>()
            {
                new Player { Username = "Player1" },
                new Player { Username = "Player2" },
                new Player { Username = "Player3" }
            };
            var gameRoom = new GameRoom
            {
                Name = "gameroom1",
                Players = new List<Player> { players[1], players[2] }
            };

            _context.AddRange(players);
            _context.Add(gameRoom);

            await _context.SaveChangesAsync();

            var mocks = players.Select(x => CreateMockPlayer(x)).ToList();
            foreach (var mock in mocks)
            {
                server.AddPlayerConnection(mock.Object);
            }

            return mocks;
        }
    }
}
