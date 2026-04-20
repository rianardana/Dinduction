// Dinduction.Application/Interfaces/IUserLearningProgressService.cs
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces;

public interface IUserLearningProgressService
{
    Task<UserLearningProgress?> GetProgressAsync(int userId, int? materialId);
    Task<IEnumerable<UserLearningProgress>> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserLearningProgress>> GetByMaterialIdAsync(int materialId);
    
    
    Task<UserLearningProgress> StartLearningAsync(int userId, int? materialId);
    Task<UserLearningProgress> UpdateProgressAsync(int userId, int? materialId, bool isStarted);
    Task<UserLearningProgress> MarkCompletedAsync(int userId, int? materialId, int trainingYear);
    
    Task<bool> IsCompletedAsync(int userId, int? materialId);
    Task<int?> GetCompletionPercentAsync(int userId, int? materialId);
    Task<bool> IsDoneAsync(int userId, int materialId, int trainingYear);
}