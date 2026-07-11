using IManager.Web.Domain.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace IManager.Web.Data.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task CommitAsync()
        => await _transaction!.CommitAsync();

    public async Task RollbackAsync()
        => await _transaction!.RollbackAsync();
}