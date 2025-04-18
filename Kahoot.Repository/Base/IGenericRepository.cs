using System.Linq.Expressions;

namespace Kahoot.Repository.Base
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {

        Task<TEntity> GetByIdAsync(object id);
        IQueryable<TEntity> GetByWhere(Expression<Func<TEntity, bool>> predicate);

        Task Attach(TEntity entity);

        Task AddAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        Task UpdateRangeAsync(IEnumerable<TEntity> entities);

        Task RemoveRange(IEnumerable<TEntity> entities);

        Task<int> CountAsync();

        Task<IEnumerable<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null);
    }
}
