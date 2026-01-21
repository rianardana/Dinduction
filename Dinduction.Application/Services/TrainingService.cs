using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;

namespace Dinduction.Infrastructure.Services;

public class TrainingService : ITrainingService
{
    private readonly IUnitOfWork _uow;

    public TrainingService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<MasterTraining>> GetAllAsync()
    {
        return await _uow.Repository<MasterTraining>().GetAllAsync(
            orderBy: t => t.TrainingName);
    }

    public async Task<MasterTraining?> GetByIdAsync(int id)
    {
        return await _uow.Repository<MasterTraining>().GetAsync(t => t.Id == id);
    }

    public async Task InsertAsync(MasterTraining obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<MasterTraining>().Add(obj);
        await _uow.SaveChangesAsync(); 
    }

    public async Task UpdateAsync(MasterTraining obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<MasterTraining>().Update(obj);
        await _uow.SaveChangesAsync(); 
    }

    public async Task DeleteAsync(int id)
    {
        var training = await GetByIdAsync(id);
        if (training != null)
        {
            _uow.Repository<MasterTraining>().Delete(training);
            await _uow.SaveChangesAsync(); 
        }
    }

    public async Task<List<MasterTraining>> GetByTrainerAsync(int sectionId)
    {
        return await _uow.Repository<MasterTraining>()
            .GetAllAsync(predicate: t => t.SectionId == sectionId && t.IsActive==true,
                        orderBy: t => t.TrainingName);
    }

    public async Task<string?> GetTrainingNameAsync(int id)
    {
        var training = await _uow.Repository<MasterTraining>()
            .GetAsync(t => t.Id == id);
        return training?.TrainingName;
    }
}