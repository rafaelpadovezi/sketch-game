using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sketch.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Database
{
    public class SketchDbContext : DbContext
    {
        public SketchDbContext(DbContextOptions<SketchDbContext> options) : base(options)
        {
        }

        public DbSet<GameRoom> GameRooms => Set<GameRoom>();
        public DbSet<Round> Rounds => Set<Round>();
        public DbSet<Turn> Turns => Set<Turn>();
        public DbSet<Word> Words => Set<Word>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<PlayerTurn> PlayersTurns => Set<PlayerTurn>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            AutomaticallyAddCreatedAndUpdatedAt();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AutomaticallyAddCreatedAndUpdatedAt();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AutomaticallyAddCreatedAndUpdatedAt()
        {
            var entitiesOnDbContext = ChangeTracker.Entries<BaseEntity>();

            if (entitiesOnDbContext == null)
            {
                return;
            }

            // createdAt addition
            foreach (var item in entitiesOnDbContext.Where(t => t.State == EntityState.Added))
            {
                item.Entity.UpdatedAt = item.Entity.CreatedAt = System.DateTime.Now;
            }

            // updatedAt addition
            foreach (var item in entitiesOnDbContext.Where(t => t.State == EntityState.Modified))
            {
                item.Entity.UpdatedAt = System.DateTime.Now;
            }
        }
    }
}