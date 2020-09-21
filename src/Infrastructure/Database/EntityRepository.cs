using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Sketch.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database
{
    public class EntityRepository<T> : IRepository<T> where T : BaseEntity
    {
        public EntityRepository(SketchDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        protected SketchDbContext Context { get; }
        protected DbSet<T> DbSet { get; }

        public virtual async ValueTask<T?> Get(Expression<Func<T, bool>> filterPredicate) =>
            await DbSet.SingleOrDefaultAsync(filterPredicate);

        public virtual async ValueTask<T?> GetById(Guid id) =>
            await DbSet.FindAsync(id);
    }
}
