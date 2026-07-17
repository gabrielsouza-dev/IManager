using System.Linq.Expressions;
namespace IManager.Web.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    // READ
    Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include = null);
    Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            int? page = null,
            int? pageSize = null);
    Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null);
    Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null);
    // WRITE
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity, Guid? audictorId = null);
    Task SoftDeleteAsync(Guid id);
    Task SoftDeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    // UTILS
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Func<IQueryable<T>, IQueryable<T>>? query = null);
}