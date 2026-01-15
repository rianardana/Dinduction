
using Dinduction.Application.Interfaces;
using System.Linq.Expressions;
using Dinduction.Domain.Entities;

namespace Dinduction.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _uow;

    public RoleService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _uow.Repository<Role>().GetAllAsync(
            orderBy: r => r.RoleName);
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _uow.Repository<Role>().GetAsync(r => r.Id == id);
    }

    public async Task InsertAsync(Role obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<Role>().Add(obj);
        _uow.SaveChanges();
    }

    public async Task UpdateAsync(Role obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<Role>().Update(obj);
        _uow.SaveChanges();
    }

    public async Task DeleteAsync(int id)
    {
        var role = await GetByIdAsync(id);
        if (role != null)
        {
            _uow.Repository<Role>().Delete(role);
            _uow.SaveChanges();
        }
    }
}