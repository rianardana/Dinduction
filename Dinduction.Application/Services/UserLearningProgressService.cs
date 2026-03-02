// Dinduction.Infrastructure/Services/UserLearningProgressService.cs
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using System.Linq;

namespace Dinduction.Infrastructure.Services;

public class UserLearningProgressService : IUserLearningProgressService
{
    private readonly IUnitOfWork _uow;

    public UserLearningProgressService(IUnitOfWork uow) => _uow = uow;

    public async Task<UserLearningProgress?> GetProgressAsync(int userId, int? materialId)
    {
        return await _uow.Repository<UserLearningProgress>().GetAsync(
            x => x.UserId == userId && x.MaterialId == materialId);
    }

    public async Task<IEnumerable<UserLearningProgress>> GetByUserIdAsync(int userId)
    {
        var all = await _uow.Repository<UserLearningProgress>().GetAllAsync();
        return all.Where(x => x.UserId == userId).ToList();
    }

    public async Task<IEnumerable<UserLearningProgress>> GetByMaterialIdAsync(int materialId)
    {
        var all = await _uow.Repository<UserLearningProgress>().GetAllAsync();
        return all.Where(x => x.MaterialId == materialId).ToList();
    }

    public async Task<UserLearningProgress> StartLearningAsync(int userId, int? materialId)
    {
        var existing = await GetProgressAsync(userId, materialId);
        if (existing != null) return existing;

        var newProgress = new UserLearningProgress
        {
            UserId = userId,
            MaterialId = materialId,
            IsCompleted = false,
            CompletedDate = null
        };
        
        _uow.Repository<UserLearningProgress>().Add(newProgress);
        await _uow.SaveChangesAsync();
        return newProgress;
    }

    public async Task<UserLearningProgress> UpdateProgressAsync(int userId, int? materialId, bool isStarted)
    {
        var progress = await GetProgressAsync(userId, materialId);
        if (progress == null) return await StartLearningAsync(userId, materialId);
        
        // ✅ Entity gak ada IsStarted field, jadi skip update
        _uow.Repository<UserLearningProgress>().Update(progress);
        await _uow.SaveChangesAsync();
        return progress;
    }

    public async Task<UserLearningProgress> MarkCompletedAsync(int userId, int? materialId,int trainingYear)
    {
        var progress = await GetProgressAsync(userId, materialId);
        
        if (progress == null)
        {
            progress = new UserLearningProgress
            {
                UserId = userId,
                MaterialId = materialId,
                TrainingYear = trainingYear,
                IsCompleted = true,
                CompletedDate = DateTime.Now
            };
            _uow.Repository<UserLearningProgress>().Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedDate = DateTime.Now;
            _uow.Repository<UserLearningProgress>().Update(progress);
        }
        
        await _uow.SaveChangesAsync();
        return progress;
    }

    public async Task<bool> IsCompletedAsync(int userId, int? materialId)
    {
        var progress = await GetProgressAsync(userId, materialId);
        return progress?.IsCompleted == true;
    }

    public async Task<int?> GetCompletionPercentAsync(int userId, int? materialId)
    {
        var progress = await GetProgressAsync(userId, materialId);
        if (progress == null) return 0;
        return progress.IsCompleted == true ? 100 : 0;
    }
    public async Task<bool> IsDoneAsync(int userId, int materialId, int trainingYear)
    {
        var progress = await _uow.Repository<UserLearningProgress>().GetAsync(
            x => x.UserId == userId 
            && x.MaterialId == materialId 
            && x.TrainingYear == trainingYear);
        
        return progress?.IsCompleted == true; 
    }
}