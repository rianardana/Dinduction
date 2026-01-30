using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dinduction.Application.DTOs;
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;

namespace Dinduction.Infrastructure.Services;

public class ParticipantService : IParticipantService
{
    private readonly IUnitOfWork _uow;

    public ParticipantService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<int> GetParticipantAsync(int userId)
    {
        var participant = await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .FirstOrDefault(c => c.UserId == userId)
        );
        return participant?.Id ?? 0;
    }

    public async Task<int> GetTrainerAsync(int participantId, int trainingId)
    {
        var participant = await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .FirstOrDefault(c => c.Id == participantId && c.TrainingId == trainingId)
        );
        return participant?.TrainerId ?? 0;
    }

    public async Task<int> GetTrainerByDateAndTrainingAsync(DateTime dateTraining, int trainingId)
    {
        var targetDate = dateTraining.Date;
        var participant = await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .FirstOrDefault(c => c.TrainingDate.HasValue && c.TrainingDate.Value.Date == targetDate && c.TrainingId == trainingId)
        );
        return participant?.TrainerId ?? 0;
    }

    public async Task<int> GetTrainerIdByParticipantIdAsync(int participantId)
    {
        var participant = await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .FirstOrDefault(c => c.UserId == participantId)
        );
        return participant?.TrainerId ?? 0;
    }

    public async Task<int> GetTrainerIdByUserAndTrainingAsync(int userId, int trainingId)
    {
        var participant = await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .FirstOrDefault(c => c.UserId == userId && c.TrainingId == trainingId)
        );
        return participant?.TrainerId ?? 0;
    }

    public async Task<List<ParticipantUser>> GetUsersAsync(int trainerId)
        {
            var today = DateTime.Today;
            var participants = _uow.Repository<ParticipantUser>().Table()
            .Where(c => c.TrainerId == trainerId && c.TrainingDate.HasValue && c.TrainingDate.Value.Date == today)
            .ToList();

            // Load User (untuk UserName & EmployeeName)
            var userIds = participants
                .Where(p => p.UserId.HasValue)
                .Select(p => p.UserId.Value)
                .Distinct()
                .ToList();

            if (userIds.Any())
            {
                var users = await _uow.Repository<User>().GetAllAsync(u => userIds.Contains(u.Id));
                var userDict = users.ToDictionary(u => u.Id, u => u);

                foreach (var p in participants)
                {
                    if (p.UserId.HasValue && userDict.TryGetValue(p.UserId.Value, out var user))
                    {
                        p.User = user;
                    }
                }
            }

            return await Task.FromResult(participants);
        }

    public async Task InsertAsync(ParticipantUser obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<ParticipantUser>().Add(obj);
        await _uow.SaveChangesAsync();
    }

    public async Task<bool> IsExistAsync(int userId, int sectionId, int trainingId)
    {
        return await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .Any(c => c.UserId == userId && c.SectionTrainerId == sectionId && c.TrainingId == trainingId)
        );
    }


    public async Task<bool> IsTrainerInputAsync(int userId, int trainingId)
    {
        
        var exists = await Task.FromResult(
            _uow.Repository<ParticipantUser>()
                .Table()
                .Any(p => p.UserId == userId && p.TrainingId == trainingId && p.TrainerId.HasValue)
        );
        return exists;
    }
    public async Task<int> CountParticipantAsync(int trainerId)
    {
        var today = DateTime.Today;
        return await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .Count(c => c.TrainerId == trainerId && c.TrainingDate.HasValue && c.TrainingDate.Value.Date == today)
        );
    }

    public async Task<int> CountParticipantPresentAsync(DateTime trainingDate)
    {
        var targetDate = trainingDate.Date;
        var userIds = await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .Where(c => c.TrainingDate.HasValue && c.TrainingDate.Value.Date == targetDate)
                .Select(c => c.UserId)
                .Distinct()
                .Count()
        );
        return userIds;
    }

    public async Task<List<ParticipantUser>> GetPresenceAsync(DateTime date, int trainingId)
    {
        var targetDate = date.Date;
        if (trainingId == 0)
        {
            return await Task.FromResult(
                _uow.Repository<ParticipantUser>().Table()
                    .Where(c => c.TrainingDate.HasValue && c.TrainingDate.Value.Date == targetDate)
                    .ToList()
            );
        }
        return await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .Where(c => c.TrainingDate.HasValue && c.TrainingDate.Value.Date == targetDate && c.TrainingId == trainingId)
                .ToList()
        );
    }

    // public async Task<List<ParticipantUser>> GetPresenceByTrainerAsync(DateTime date, int trainingId, int trainerId)
    // {
    //     var targetDate = date.Date;
    //     var query = _uow.Repository<ParticipantUser>().Table().Where(c => c.TrainerId == trainerId);
    //     if (trainingId == 0)
    //     {
    //         return await Task.FromResult(
    //             query.Where(c => c.TrainingDate.HasValue && c.TrainingDate.Value.Date == targetDate).ToList()
    //         );
    //     }
    //     return await Task.FromResult(
    //         query.Where(c => c.TrainingDate.HasValue && c.TrainingDate.Value.Date == targetDate && c.TrainingId == trainingId).ToList()
    //     );
    // }

    public async Task<List<ParticipantUser>> GetPresenceByTrainerAsync(DateTime date, int trainingId, int trainerId)
    {
        var targetDate = date.Date;
        
        Expression<Func<ParticipantUser, bool>> predicate;
        
        if (trainingId == 0)
        {
            predicate = c => c.TrainerId == trainerId 
                && c.TrainingDate.HasValue 
                && c.TrainingDate.Value.Date == targetDate;
        }
        else
        {
            predicate = c => c.TrainerId == trainerId 
                && c.TrainingDate.HasValue 
                && c.TrainingDate.Value.Date == targetDate 
                && c.TrainingId == trainingId;
        }
        
        // ✅ Include dengan string path (support ThenInclude)
        var data = await _uow.Repository<ParticipantUser>()
            .GetAllWithIncludesAsync(predicate: predicate,orderBy: null,includeProperties: new[]
                {
                    "User",           // Include User
                    "Training",       // Include Training
                    "Trainer",        // Include Trainer
                    "Trainer.User"    // ✅ ThenInclude Trainer.User
                }
            );
        
        return data;
    }


    public async Task<List<DateTime>> GetTrainingDatesAsync()
    {
        var dates = await Task.FromResult(
            _uow.Repository<ParticipantUser>().Table()
                .Where(t => t.TrainingDate.HasValue)
                .Select(t => t.TrainingDate.Value.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList()
        );
        return dates;
    }

    public async Task<List<DateTime>> GetTrainingDatesByTrainerAsync(int trainerId)
    {
        
        var dates = await Task.Run(() => 
            _uow.Repository<ParticipantUser>().Table()
                .Where(t => t.TrainerId == trainerId && t.TrainingDate.HasValue)
                .Select(t => t.TrainingDate.Value.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList()
        );
        return dates;
    }

    public async Task<List<TrainingDateDTO>> GetTrainingGroupedByDateAsync()
{
    var all = await Task.FromResult(
        _uow.Repository<ParticipantUser>().Table()
            .Where(t => t.TrainingDate.HasValue && t.TrainingId.HasValue)
            .ToList()
    );

    return all
        .GroupBy(p => p.TrainingDate.Value.Date)
        .Select(g => new TrainingDateDTO
        {
            Date = g.Key,
            Trainings = g
                .GroupBy(p => p.TrainingId.Value)
                .Select(tg => new TrainingDto
                {
                    TrainingId = tg.Key,
                    // ✅ GANTI INI:
                    TrainingName = tg.FirstOrDefault()?.Training?.TrainingName ?? "Unknown",
                    TrainingDate = g.Key
                })
                .ToList()
        })
        .OrderBy(dto => dto.Date)
        .ToList();
}

        public async Task<List<TrainingDateDTO>> GetTrainingGroupedByDateByTrainerAsync(int trainerId)
        {
            var result = await Task.Run(() =>
                _uow.Repository<ParticipantUser>().Table()
                    .Where(t => t.TrainerId == trainerId && t.TrainingDate.HasValue && t.TrainingId.HasValue)
                    .GroupBy(t => t.TrainingDate.Value.Date)
                    .Select(g => new TrainingDateDTO
                    {
                        Date = g.Key,
                        Trainings = g
                            .GroupBy(p => p.TrainingId.Value)
                            .Select(tg => new TrainingDto
                            {
                                TrainingId = tg.Key,
                                TrainingName = tg.FirstOrDefault().Training.TrainingName,
                                TrainingDate = g.Key
                            })
                            .ToList()
                    })
                    .OrderBy(dto => dto.Date)
                    .ToList()
            );

            return result;
        }

        public async Task<List<ParticipantUser>> GetByTrainerWithDetailsAsync(
            int trainerId, 
            DateTime date, 
            int? trainingId = null)
        {
            // ✅ Build predicate
            Expression<Func<ParticipantUser, bool>> predicate;

            if (trainingId.HasValue && trainingId.Value != 0)
            {
                predicate = p => p.TrainerId == trainerId
                    && p.TrainingDate.HasValue
                    && p.TrainingDate.Value.Date == date.Date
                    && p.TrainingId == trainingId.Value;
            }
            else
            {
                predicate = p => p.TrainerId == trainerId
                    && p.TrainingDate.HasValue
                    && p.TrainingDate.Value.Date == date.Date;
            }

            // ✅ Pakai GetAllWithIncludesAsync - NO NEED EntityFrameworkCore using!
            return await _uow.Repository<ParticipantUser>()
                .GetAllWithIncludesAsync(
                    predicate: predicate,
                    orderBy: null,
                    includeProperties: new[]
                    {
                        "User",
                        "Training",
                        "Trainer",
                        "Trainer.User"
                    }
                );
        }
}