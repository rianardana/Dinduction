using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dinduction.Domain.Entities;

namespace Dinduction.Application.Interfaces
{
    public interface ITrainingService
{
    Task<List<MasterTraining>> GetAllAsync();
    Task<MasterTraining?> GetByIdAsync(int id);
    Task InsertAsync(MasterTraining obj);
    Task UpdateAsync(MasterTraining obj);
    Task DeleteAsync(int id);
    Task<List<MasterTraining>> GetByTrainerAsync(int sectionId);
    Task<string?> GetTrainingNameAsync(int id);
}
}