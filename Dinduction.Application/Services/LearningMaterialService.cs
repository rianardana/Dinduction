using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using System.Linq;

namespace Dinduction.Infrastructure.Services;

public class LearningMaterialService : ILearningMaterialService
{
    private readonly IUnitOfWork _uow;

    public LearningMaterialService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<LearningMaterial>> GetAllAsync()
    {
        var all = await _uow.Repository<LearningMaterial>().GetAllAsync();
        return all.Where(x => x.IsActive ?? true).ToList();
    }

    public async Task<LearningMaterial?> GetByIdAsync(int id)
    {
        var all = await _uow.Repository<LearningMaterial>().GetAllAsync();
        return all.FirstOrDefault(x => x.Id == id && (x.IsActive ?? true));
    }

    public async Task<IEnumerable<LearningMaterial>> GetByTrainingIdAsync(int trainingId)
    {
        var all = await _uow.Repository<LearningMaterial>().GetAllAsync();
        return all.Where(x => x.TrainingId == trainingId && (x.IsActive ?? true)).ToList();
    }

    public async Task<LearningMaterial> CreateAsync(LearningMaterial entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        entity.IsActive = true;
        
        _uow.Repository<LearningMaterial>().Add(entity);
        await _uow.SaveChangesAsync();
        
        return entity;
    }

    public async Task<LearningMaterial> UpdateAsync(LearningMaterial entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        var existing = await GetByIdAsync(entity.Id);
        if (existing == null) throw new ApplicationException($"Material with ID {entity.Id} not found");

        existing.FilePath = entity.FilePath;
        existing.TrainingId = entity.TrainingId;
        
        _uow.Repository<LearningMaterial>().Update(existing);
        await _uow.SaveChangesAsync();
        
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;

        entity.IsActive = false;
        
        _uow.Repository<LearningMaterial>().Update(entity);
        await _uow.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> IsActiveAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        return entity?.IsActive ?? false;
    }
}