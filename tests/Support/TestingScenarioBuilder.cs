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

        public async Task<IEnumerable<Mock<IPlayerConnection>>> BuildScenarioWith3ConnectedPlayers(
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

            var mocks = players.Select(x =>
            {
                var mock = new Mock<IPlayerConnection>();
                mock.Setup(m => m.Id).Returns(x.Id);
                mock.Setup(m => m.IsConnected).Returns(true);
                return mock;
            }).ToList();
            foreach (var mock in mocks)
            {
                server.AddPlayerConnection(mock.Object);
            }

            return mocks;
        }
    }
}
