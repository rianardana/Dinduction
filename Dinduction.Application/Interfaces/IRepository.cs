
using System.Linq.Expressions;

namespace Dinduction.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);

    // Raw SQL (opsional)
    void ExecuteSql(string sql);
    List<T> SqlQueryList(string sql);
    T? SqlQuery(string sql);
    IQueryable<T> Table();
}