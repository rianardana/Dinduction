// Dinduction.Infrastructure/UnitOfWork/UnitOfWork.cs
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

    public void SaveChanges()
    {
        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            // Optional: log error
            throw new Exception("Error saving changes.", ex);
        }
    }

    public void CreateTransaction()
    {
        _transaction = _context.Database.BeginTransaction();
    }

    public void Commit()
    {
        _transaction?.Commit();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
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
}