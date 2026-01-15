using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces;
public interface IRoleService
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task InsertAsync(Role obj);
    Task UpdateAsync(Role obj);
    Task DeleteAsync(int id);
}