using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Domain.Domain_Models;

namespace Repository.Interface
{
    public interface IRepository<T> where T : BaseEntity
    {
        T Insert(T entity);
        List<T> InsertMany(List<T> entities);
        T Update(T entity);
        T Delete(T entity);
        E? Get<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        IEnumerable<E> GetAll<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
    }
}