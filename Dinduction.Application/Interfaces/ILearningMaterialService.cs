using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces;

public interface ILearningMaterialService
{
    Task<IEnumerable<LearningMaterial>> GetAllAsync();
    Task<LearningMaterial?> GetByIdAsync(int id);
    Task<IEnumerable<LearningMaterial>> GetByTrainingIdAsync(int trainingId);
    Task<LearningMaterial> CreateAsync(LearningMaterial entity);
    Task<LearningMaterial> UpdateAsync(LearningMaterial entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsActiveAsync(int id);
}