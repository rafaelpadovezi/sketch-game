using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public class SketchDbContext : DbContext
    {
        public SketchDbContext(DbContextOptions<SketchDbContext> options) : base(options)
        {
        }
    }
}