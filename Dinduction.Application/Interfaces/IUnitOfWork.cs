namespace Dinduction.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    void SaveChanges();
    void CreateTransaction();
    void Commit();
    void Rollback();
}