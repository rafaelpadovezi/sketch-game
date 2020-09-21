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
    }
}
