using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database.Repositories
{
    public class PlayerRepository : EntityRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(SketchDbContext context) : base(context)
        {
        }

        public async Task<bool> NicknameIsInUse(string username) =>
            await DbSet
                .Where(x => x.IsActive)
                .Where(x => x.Username.ToLower() == username.ToLower())
                .AnyAsync();
    }
}
