using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database
{
    public interface IRepository<T> where T : BaseEntity
    {
        ValueTask<T?> GetById(Guid id);
        ValueTask<T?> Get(Expression<Func<T, bool>> filterPredicate);
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> filterPredicate);
        void Add(T entity);
        void Update(T entity);
        Task SaveChanges();
        Task AddAndSave(T entity);
        Task UpdateAndSave(T entity);
    }
}
