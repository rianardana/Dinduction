// ITrainerService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces
{
    public interface ITrainerService
    {
        Task<List<Trainer>> GetAllAsync();
        Task<int> GetTrainerIdAsync(int userId);
        Task<int> GetSectionTrainerIdAsync(int userId);
        Task InsertAsync(Trainer trainer);
        Task<int> GetUserIdByTrainerIdAsync(int trainerId);
        Task<int> CountTrainingAsync(int sectionTrainerId);
        Task<int> CountTrainingForAdminAsync();
        Task<Trainer?> GetByUserIdAsync(int userId);
    }
}