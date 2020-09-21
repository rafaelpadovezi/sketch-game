using Infrastructure.Database;
using Sketch.Models;
using System;
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

        public async Task<Player> BuildScenarioPlayer(string username)
        {
            Player player = new Player { Username = username };
            _context.Add(player);

            await _context.SaveChangesAsync();

            return player;
        }
    }
}
