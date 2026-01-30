using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dinduction.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

        // extended GetAllAsync: now supports optional includes (navigation properties)
        Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);

        // Raw SQL (optional)
        void ExecuteSql(string sql);
        List<T> SqlQueryList(string sql);
        T? SqlQuery(string sql);
        IQueryable<T> Table();
        Task<List<T>> GetAllWithIncludesAsync(Expression<Func<T, bool>>? predicate = null,Expression<Func<T, object>>? orderBy = null,params string[] includeProperties);

    }
}