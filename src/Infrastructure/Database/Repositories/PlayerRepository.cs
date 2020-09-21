using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Models;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database.Repositories
{
    public class PlayerRepository : EntityRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(SketchDbContext context) : base(context)
        {
        }

        public async Task<bool> NicknameIsInUse(string username) =>
            await DbSet.AnyAsync(x => x.Username.ToLower() == username.ToLower());

        public async Task Add(Player player)
        {
            DbSet.Add(player);
            await Context.SaveChangesAsync();
        }
    }
}
