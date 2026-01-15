
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces;
public interface ISectionService
{
    Task<List<Section>> GetAllAsync();
    Task<Section?> GetByIdAsync(int id);
    Task InsertAsync(Section obj);
    Task UpdateAsync(Section obj);
    Task DeleteAsync(int id);
}