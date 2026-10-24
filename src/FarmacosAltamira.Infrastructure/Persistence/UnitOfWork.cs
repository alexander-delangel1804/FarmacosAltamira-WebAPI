using FarmacosAltamira.Application.Common.Interfaces;
using FarmacosAltamira.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace FarmacosAltamira.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CentralDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(CentralDbContext context)
    {
        _context = context;
    }

    public ICentralDbContext Context => _context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}