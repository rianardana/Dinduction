using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dinduction.Application.Interfaces;
using Dinduction.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dinduction.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (_repositories.TryGetValue(type, out var repo))
        {
            return (IRepository<T>)repo;
        }

        var newRepo = new EfRepository<T>(_context);
        _repositories[type] = newRepo;
        return newRepo;
    }

    // Synchronous save (backwards compatibility)
    public void SaveChanges()
    {
        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            // Optional: replace with logging
            throw new Exception("Error saving changes.", ex);
        }
    }

    // Async save
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Optional: replace with logging
            throw new Exception("Error saving changes.", ex);
        }
    }

    // Sync transaction
    public void CreateTransaction()
    {
        _transaction = _context.Database.BeginTransaction();
    }

    // Async transaction
    public async Task CreateTransactionAsync(System.Data.IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default)
    {
        _transaction = isolationLevel.HasValue
            ? await _context.Database.BeginTransactionAsync(isolationLevel.Value, cancellationToken)
            : await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public void Commit()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Async dispose to properly dispose DbContext and transaction
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}