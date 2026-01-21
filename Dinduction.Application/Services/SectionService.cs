using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using System.Linq.Expressions;

namespace Dinduction.Infrastructure.Services; // 👈 pastikan namespace sesuai konvensi

public class SectionService : ISectionService
{
    private readonly IUnitOfWork _uow;

    public SectionService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
        await _uow.SaveChangesAsync(); 
    }

    public async Task UpdateAsync(Section obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        _uow.Repository<Section>().Update(obj);
        await _uow.SaveChangesAsync(); 
    }

    public async Task DeleteAsync(int id)
    {
        var section = await GetByIdAsync(id);
        if (section != null)
        {
            _uow.Repository<Section>().Delete(section);
            await _uow.SaveChangesAsync(); 
        }
    }
}