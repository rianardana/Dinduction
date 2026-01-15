using System;
using System.Threading;
using System.Threading.Tasks;
using Dinduction.Application.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IRepository<T> Repository<T>() where T : class;

    // sync (tetap jika ada penggunaan lama)
    void SaveChanges();

    // async
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Transaction (sync + async)
    void CreateTransaction();
    Task CreateTransactionAsync(System.Data.IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default);

    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default);

    void Rollback();
    Task RollbackAsync(CancellationToken cancellationToken = default);
}