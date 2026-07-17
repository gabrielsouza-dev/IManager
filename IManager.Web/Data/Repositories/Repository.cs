using IManager.Web.Data.Persistence;
using IManager.Web.Domain.Entities;
using IManager.Web.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace IManager.Web.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }
    public async Task<int> CountAsync(Func<IQueryable<T>, IQueryable<T>>? queryBuilder = null)
    {
        IQueryable<T> query = _dbSet;
        if(queryBuilder != null)
        {
            query = queryBuilder(query);
        }

        return await query.CountAsync();
    }
    public async Task SoftDeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
            await SoftDeleteAsync(entity);
    }
    public async Task SoftDeleteAsync(T entity)
    {
        entity.Deactivate();
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);
    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _dbSet;
        if (include != null)
            query = include(query);
        return await query.Where(predicate).ToListAsync();
    }
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _dbSet;
        if (include != null)
            query = include(query);
        return await query.FirstOrDefaultAsync(predicate);
    }
    public async Task<IEnumerable<T>> GetAllAsync(
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        int? page = null,
        int? pageSize = null)
    {
        IQueryable<T> query = _dbSet;

        if (include != null)
            query = include(query);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .OrderBy(e => EF.Property<object>(e, "Id"))
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }
    public async Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        if (include == null)
            return await _dbSet.FindAsync(id);
        return await include(_dbSet).FirstOrDefaultAsync(e => e.Id == id);
    }
    public async Task UpdateAsync(T entity, Guid? audictorId = null)
    {
        if(audictorId != null)
            entity.Touch(audictorId);
        
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }
}