using Dinduction.Application.DTOs;
using Dinduction.Domain.Entities;
namespace Dinduction.Application.Interfaces;
public interface IParticipantService
{
    Task<int> GetParticipantAsync(int userId);
    Task<int> GetTrainerAsync(int participantId, int trainingId);
    Task<int> GetTrainerByDateAndTrainingAsync(DateTime dateTraining, int trainingId);
    Task<int> GetTrainerIdByParticipantIdAsync(int participantId);
    Task<int> GetTrainerIdByUserAndTrainingAsync(int userId, int trainingId);
    Task<List<ParticipantUser>> GetUsersAsync(int trainerId);
    Task InsertAsync(ParticipantUser obj);
    Task<bool> IsExistAsync(int userId, int sectionId, int trainingId);
    Task<bool> IsTrainerInputAsync(int userId, int trainingId);
    Task<int> CountParticipantAsync(int trainerId);
    Task<int> CountParticipantPresentAsync(DateTime trainingDate);
    Task<List<ParticipantUser>> GetPresenceAsync(DateTime date, int trainingId);
    Task<List<ParticipantUser>> GetPresenceByTrainerAsync(DateTime date, int trainingId, int trainerId);
    Task<List<DateTime>> GetTrainingDatesAsync();
    Task<List<DateTime>> GetTrainingDatesByTrainerAsync(int trainerId);
    Task<List<TrainingDateDTO>> GetTrainingGroupedByDateAsync();
    Task<List<TrainingDateDTO>> GetTrainingGroupedByDateByTrainerAsync(int trainerId);
}