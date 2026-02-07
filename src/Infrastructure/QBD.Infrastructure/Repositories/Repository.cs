using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Common;
using QBD.Infrastructure.Data;

namespace QBD.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly QBDesktopDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(QBDesktopDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(T entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public IQueryable<T> Query()
    {
        return DbSet.AsQueryable();
    }

    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false)
    {
        IQueryable<T> query = DbSet;

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync();

        if (orderBy != null)
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        else
            query = query.OrderBy(e => e.Id);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
