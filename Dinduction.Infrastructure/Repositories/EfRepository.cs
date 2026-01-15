using System.Linq.Expressions;
using Dinduction.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dinduction.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate) 
        => await _dbSet.FirstOrDefaultAsync(predicate);

    public async Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null)
    {
        var query = _dbSet.AsQueryable();
        if (predicate != null) query = query.Where(predicate);
        if (orderBy != null) query = query.OrderBy(orderBy);
        return await query.ToListAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.CountAsync(predicate);

    public void Add(T entity) => _dbSet.Add(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);

    // === Raw SQL ===
    public void ExecuteSql(string sql) => _context.Database.ExecuteSqlRaw(sql);

    public List<T> SqlQueryList(string sql) 
        => _context.Set<T>().FromSqlRaw(sql).ToList();

    public T? SqlQuery(string sql) 
        => _context.Set<T>().FromSqlRaw(sql).FirstOrDefault();
    
    public IQueryable<T> Table()
    {
        return _dbSet.AsNoTracking();
    }
}