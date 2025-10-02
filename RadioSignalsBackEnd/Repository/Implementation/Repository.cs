using System.Linq.Expressions;
using Domain.Domain_Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Repository.Interface;

namespace Repository.Implementation
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> entites;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            this.entites = _context.Set<T>();
        }

        public async Task<T> InsertAsync(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<T>> InsertManyAsync(List<T> entities)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities));
            }

            await _context.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        

        public async Task<T> DeleteAsync(T entity, Guid userId) 
        {
            var deletedAtProperty = typeof(T).GetProperty("DeletedAt");
            var deletedByProperty = typeof(T).GetProperty("DeletedBy");

            if (deletedAtProperty != null && deletedAtProperty.PropertyType == typeof(DateTime?))
            {
                deletedAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            if (deletedByProperty != null && deletedByProperty.PropertyType == typeof(Guid?))
            {
                deletedByProperty.SetValue(entity, userId);
            }

            _context.Update(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<E?> GetAsync<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = entites;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).Select(selector).FirstOrDefaultAsync();
            return await query.Select(selector).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = entites;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).Select(selector).ToListAsync();
            return await query.Select(selector).ToListAsync();
        }
    }
}