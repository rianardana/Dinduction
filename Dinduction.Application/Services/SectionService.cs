
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Services;

public class SectionService : ISectionService
{
    private readonly IUnitOfWork _uow;

    public SectionService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Section>> GetAllAsync()
    {
        return await _uow.Repository<Section>().GetAllAsync(
            orderBy: c => c.SectionName);
    }

    public async Task<Section?> GetByIdAsync(int id)
    {
        return await _uow.Repository<Section>().GetAsync(c => c.Id == id);
    }

    public async Task InsertAsync(Section obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        _uow.Repository<Section>().Add(obj);
        _uow.SaveChanges();
    }

    public async Task UpdateAsync(Section obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        _uow.Repository<Section>().Update(obj);
        _uow.SaveChanges();
    }

    public async Task DeleteAsync(int id)
    {
        var section = await GetByIdAsync(id);
        if (section != null)
        {
            _uow.Repository<Section>().Delete(section);
            _uow.SaveChanges();
        }
    }
}