using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database
{
    public class EntityRepository<T> : IRepository<T> where T : BaseEntity, new()
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

        public virtual async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> filterPredicate) =>
            await DbSet.Where(filterPredicate).ToListAsync();

        public virtual async ValueTask<T?> GetById(Guid id) =>
            await DbSet.FindAsync(id);

        public virtual async Task AddAndSave(T entity)
        {
            DbSet.Add(entity);
            await SaveChanges();
        }

        public virtual async Task UpdateAndSave(T entity)
        {
            DbSet.Update(entity);
            await SaveChanges();
        }

        public virtual void Add(T entity) => DbSet.Add(entity);

        public virtual void Update(T entity) => DbSet.Update(entity);

        public virtual void Remove(Guid id)
        {
            var entity = new T
            {
                Id = id
            };
            Remove(entity);
        }

        public virtual void Remove(T entity)
        {
            DbSet.Remove(entity);
        }

        public virtual async Task RemoveAndSave(Guid id)
        {
            Remove(id);
            await SaveChanges();
        }

        public virtual async Task RemoveAndSave(T entity)
        {
            Remove(entity);
            await SaveChanges();
        }

        public virtual async Task SaveChanges() =>
            await Context.SaveChangesAsync();
    }
}
