﻿using Kahoot.Repository.Base;
using Kahoot.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Kahoot.Repository.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly KahootContext _context = null;

        public GenericRepository()
        {
            this._context = new KahootContext();
        }

        public GenericRepository(KahootContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().AsQueryable().AsNoTracking();
        }

        public async Task Attach(TEntity entity)
        {
            _context.Set<TEntity>().Attach(entity);
            _context.Entry(entity).State = EntityState.Unchanged;
        }


        public async Task<TEntity> GetByIdAsync(object id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public IQueryable<TEntity> GetByWhere(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Where(predicate);
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().UpdateRange(entities);
        }


        public async Task DeleteAsync(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task RemoveRange(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<TEntity>().CountAsync();
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (include != null)
            {
                query = include(query);
            }
            if (pageNumber == 0) pageNumber = 1;
            if (pageSize == 0) pageSize = 10;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            return await query.Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .AsNoTracking()
                              .ToListAsync();
        }
    }
}
