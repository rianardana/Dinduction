using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces
{
    public interface ITrainerService
    {
        Task<List<Trainer>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<int> GetTrainerIdAsync(int userId, CancellationToken cancellationToken = default);

        Task<int> GetSectionTrainerIdAsync(int userId, CancellationToken cancellationToken = default);

        Task InsertAsync(Trainer trainer, CancellationToken cancellationToken = default);

        Task<int> GetUserIdByTrainerIdAsync(int trainerId, CancellationToken cancellationToken = default);

        Task<int> CountTrainingAsync(int sectionTrainerId, CancellationToken cancellationToken = default);

        Task<int> CountTrainingForAdminAsync(CancellationToken cancellationToken = default);
    }
}