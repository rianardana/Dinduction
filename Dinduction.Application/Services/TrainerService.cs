// TrainerService.cs
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;

namespace Dinduction.Infrastructure.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _uow;

        public TrainerService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<List<Trainer>> GetAllAsync()
        {
            return await _uow.Repository<Trainer>()
                .GetAllAsync(
                    predicate: null,
                    orderBy: t => t.UserId,
                    includes: new Expression<Func<Trainer, object>>[] { t => t.Section!, t => t.User! }
                );
        }

        public async Task<int> GetTrainerIdAsync(int userId)
        {
            var trainer = await _uow.Repository<Trainer>().GetAsync(t => t.UserId == userId);
            if (trainer == null)
                throw new InvalidOperationException($"Trainer dengan UserId {userId} tidak ditemukan.");

            return trainer.Id;
        }

        public async Task<int> GetSectionTrainerIdAsync(int userId)
        {
            var trainer = await _uow.Repository<Trainer>().GetAsync(t => t.UserId == userId);
            if (trainer == null)
                throw new InvalidOperationException($"Trainer dengan UserId {userId} tidak ditemukan.");

            if (!trainer.SectionId.HasValue)
                throw new InvalidOperationException($"Trainer dengan UserId {userId} tidak memiliki SectionId.");

            return trainer.SectionId.Value;
        }

        public async Task InsertAsync(Trainer obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            _uow.Repository<Trainer>().Add(obj);
            await _uow.SaveChangesAsync();
        }

        public async Task<int> GetUserIdByTrainerIdAsync(int trainerId)
        {
            var trainer = await _uow.Repository<Trainer>().GetByIdAsync(trainerId);
            if (trainer == null)
                throw new InvalidOperationException($"Trainer dengan Id {trainerId} tidak ditemukan.");

            if (!trainer.UserId.HasValue)
                throw new InvalidOperationException($"Trainer dengan Id {trainerId} tidak memiliki UserId.");

            return trainer.UserId.Value;
        }

        public async Task<int> CountTrainingAsync(int sectionTrainerId)
        {
            return await _uow.Repository<MasterTraining>()
                .CountAsync(mt => mt.SectionId == sectionTrainerId && mt.IsActive == true);
        }

        public async Task<int> CountTrainingForAdminAsync()
        {
            return await _uow.Repository<MasterTraining>()
                .CountAsync(mt => mt.IsActive == true);
        }

        public async Task<Trainer?> GetByUserIdAsync(int userId)
        {
            var trainer = await _uow.Repository<Trainer>()
                .GetAsync(t => t.UserId == userId);
                    
            if (trainer != null && trainer.UserId.HasValue)
            {
                trainer.User = await _uow.Repository<User>()
                    .GetByIdAsync(trainer.UserId.Value);
            }
            
            return trainer;
        }
    }
}