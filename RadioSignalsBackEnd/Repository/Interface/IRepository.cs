// ...
using Domain.Domain_Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Repository.Interface
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T> InsertAsync(T entity);
        Task<List<T>> InsertManyAsync(List<T> entities);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(T entity);
        Task<E?> GetAsync<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        Task<IEnumerable<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
    }
}