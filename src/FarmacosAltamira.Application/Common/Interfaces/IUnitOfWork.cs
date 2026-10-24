using FarmacosAltamira.Application.Common.Interfaces;

namespace FarmacosAltamira.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICentralDbContext Context { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}