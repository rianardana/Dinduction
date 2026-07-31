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
    Console.WriteLine($"[DEBUG] Start GetPresenceAsync for Date: {targetDate}");

    var userIds = await Task.Run(() =>
    {
        var joinResult = _uow.Repository<RecordTraining>()
            .Table()
            .Join(_uow.Repository<ParticipantUser>().Table(), 
                  rt => rt.ParticipantId, 
                  pu => pu.Id, 
                  (rt, pu) => new { rt.RecordDate, rt.TrainingType, pu.UserId })
            .Where(x => x.RecordDate.HasValue 
                     && x.RecordDate.Value.Date == targetDate
                     && x.UserId.HasValue) 
            .Select(x => x.UserId.Value)
            .Distinct()
            .ToList();

        Console.WriteLine($"[DEBUG] Found {joinResult.Count} UserIds with activity on {targetDate}");
        
        if (joinResult.Contains(2066))
        {
            Console.WriteLine($"[DEBUG] SUCCESS: UserId 2066 (Fenni) FOUND in join result!");
        }
        else
        {
            Console.WriteLine($"[DEBUG] FAILURE: UserId 2066 (Fenni) NOT FOUND in join result.");
            Console.WriteLine($"[DEBUG] Sample UserIds found: {string.Join(", ", joinResult.Take(5))}");
        }

        return joinResult;
    });

    if (!userIds.Any())
    {
        Console.WriteLine("[DEBUG] No UserIds found, returning empty list.");
        return new List<ParticipantUser>();
    }

    var participants = await Task.Run(() =>
    {
        var allRelevantParticipants = _uow.Repository<ParticipantUser>()
            .Table()
            .Where(pu => pu.UserId.HasValue && userIds.Contains(pu.UserId.Value))
            .OrderByDescending(pu => pu.Id)
            .ToList();

        Console.WriteLine($"[DEBUG] Found {allRelevantParticipants.Count} raw ParticipantUser records for these UserIds.");

        var grouped = allRelevantParticipants
            .GroupBy(pu => pu.UserId.Value)
            .Select(g => g.First())
            .ToList();

        Console.WriteLine($"[DEBUG] After grouping by UserId, we have {grouped.Count} unique participants.");
        
        var fenniInList = grouped.FirstOrDefault(p => p.UserId == 2066);
        if (fenniInList != null)
        {
            Console.WriteLine($"[DEBUG] SUCCESS: Fenni (UserId 2066) is in the final participant list!");
        }
        else
        {
            Console.WriteLine($"[DEBUG] FAILURE: Fenni (UserId 2066) is MISSING from the final participant list.");
        }

        return grouped;
    });

    var userIdsToLoad = participants.Select(p => p.UserId.Value).Distinct().ToList();
    
    if (userIdsToLoad.Any())
    {
        var users = await _uow.Repository<User>().GetAllAsync(u => userIdsToLoad.Contains(u.Id));
        var userDict = users.ToDictionary(u => u.Id, u => u);

        foreach (var p in participants)
        {
            if (userDict.TryGetValue(p.UserId.Value, out var user))
            {
                p.User = user;
                p.TrainingDate = targetDate;
            }
        }
    }

    Console.WriteLine($"[DEBUG] Returning {participants.Count} participants.");
    return participants;
}


        public async Task<List<ParticipantUser>> GetPresenceByTrainerAsync(DateTime date, int trainingId, int trainerId)
{
    return await Task.Run(() =>
    {
        var records = _uow.Repository<RecordTraining>()
            .Table()
            .Where(r => r.TrainerId == trainerId
                    && r.TrainingId == trainingId
                    && r.TrainingType == "I"
                    && (r.StepType == null || r.StepType == "Normal")
                    && r.RecordDate.Value.Date == date.Date)
            .ToList();

        if (!records.Any()) return new List<ParticipantUser>();

        var participantIds = records
            .Select(r => r.ParticipantId)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToList();

        var participants = _uow.Repository<ParticipantUser>()
            .Table()
            .Where(p => participantIds.Contains(p.Id))
            .ToList();

        var userIds = participants
            .Select(p => p.UserId)
            .Distinct()
            .ToList();

        var users = _uow.Repository<User>()
            .Table()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u);

        foreach (var participant in participants)
        {
            if (users.ContainsKey(participant.UserId.Value))
            {
                participant.User = users[participant.UserId.Value];
            }
        }

        return participants;
    });
}

    
    public async Task<(List<ParticipantUser> participants, Dictionary<int, string> trainingTypes)> 
        GetPresenceByTrainerWithTrainingTypeAsync(DateTime date, int trainingId, int trainerId)
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
                && c.TrainingId == trainingId
                && c.User.TrainingType=="I";
        }
        
        // 1. Ambil participants
        var participants = await _uow.Repository<ParticipantUser>()
            .GetAllWithIncludesAsync(
                predicate: predicate,
                orderBy: null,
                includeProperties: new[] { "User", "Training", "Trainer" }
            );
        
        // 2. Ambil TrainingType dari RecordTraining
        var participantIds = participants.Select(p => p.Id).ToList();
        var trainingTypes = new Dictionary<int, string>();
        
        if (participantIds.Any())
        {
            var trainingTypeList = _uow.Repository<RecordTraining>()
                .Table()
                .Where(r => participantIds.Contains(r.ParticipantId.Value) 
                        && !string.IsNullOrEmpty(r.TrainingType))
                .GroupBy(r => r.ParticipantId)
                .Select(g => new { 
                    ParticipantId = g.Key, 
                    TrainingType = g.Select(r => r.TrainingType).FirstOrDefault() 
                })
                .ToList(); 
            
            trainingTypes = trainingTypeList
                .Where(x => x.ParticipantId.HasValue)
                .ToDictionary(
                    x => x.ParticipantId.Value, 
                    x => x.TrainingType
                );
        }
        
        // ParticipantService.cs - SEBELUM return

            Console.WriteLine($"🔍 SERVICE: participants.Count = {participants.Count}");
            Console.WriteLine($"🔍 SERVICE: trainingTypes.Count = {trainingTypes.Count}");

            foreach (var kvp in trainingTypes)
            {
                Console.WriteLine($"🔍 SERVICE: trainingTypes[{kvp.Key}] = '{kvp.Value}'");
            }

            foreach (var p in participants)
            {
                Console.WriteLine($"🔍 SERVICE: Participant Id={p.Id}, UserId={p.UserId}");
            }


        return (participants, trainingTypes);
    }


    public async Task<List<DateTime>> GetTrainingDatesAsync()
    {
        return await Task.Run(() =>
            _uow.Repository<RecordTraining>()
                .Table()
                .Where(r => r.RecordDate.HasValue) 
                .ToList()
                .Select(r => r.RecordDate.Value.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList()
        );
    }

    public async Task<List<DateTime>> GetTrainingDatesByTypeAsync(string trainingType)
    {
        return await Task.Run(() =>
            _uow.Repository<RecordTraining>()
                .Table()
                .Where(r => r.RecordDate.HasValue 
                        && r.TrainingType == trainingType)
                .Select(r => r.RecordDate.Value.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList()
        );
    }

    public async Task<List<DateTime>> GetRefreshTrainingDatesAsync()
    {
        return await Task.Run(() =>
            _uow.Repository<RecordTraining>()
                .Table()
                .Where(r => r.RecordDate.HasValue 
                        && r.TrainingType == "R") 
                .ToList()
                .Select(r => r.RecordDate.Value.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList()
        );
    }

   public async Task<List<DateTime>> GetTrainingDatesByTrainerAsync(int trainerId)
{
    return await Task.Run(() =>
        _uow.Repository<RecordTraining>()
            .Table()
            .Where(r => r.RecordDate.HasValue 
                    && r.TrainerId == trainerId
                    && r.TrainingType == "I") 
            .Select(r => r.RecordDate.Value.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList()
    );
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
                        
                        TrainingName = tg.FirstOrDefault()?.Training?.TrainingName ?? "Unknown",
                        TrainingDate = g.Key
                    })
                    .ToList()
            })
            .OrderBy(dto => dto.Date)
            .ToList();
    }

    public async Task<Dictionary<int, int>> GetOriginalParticipantIdsAsync(DateTime date, List<int> userIds)
{
    if (!userIds.Any()) return new Dictionary<int, int>();

    var mapping = new Dictionary<int, int>();

    var records = await Task.Run(() =>
        _uow.Repository<RecordTraining>()
            .Table()
            .Where(r => r.RecordDate.HasValue 
                    && r.RecordDate.Value.Date == date.Date
                    && r.TrainingType == "R")
            .ToList()
    );

    var validParticipantIds = records
        .Where(r => r.ParticipantId.HasValue)
        .Select(r => r.ParticipantId.Value)
        .Distinct()
        .ToList();
    
    if (!validParticipantIds.Any()) return mapping;

    var participants = await Task.Run(() =>
        _uow.Repository<ParticipantUser>()
            .Table()
            .Where(pu => validParticipantIds.Contains(pu.Id))
            .ToList()
    );

    var puDict = new Dictionary<int, int>();
    foreach (var p in participants)
    {
        if (p.UserId > 0)
        {
            puDict[p.Id] = p.UserId.Value;
        }
    }

    foreach (var r in records)
    {
        if (r.ParticipantId.HasValue)
        {
            int currentPid = r.ParticipantId.Value;
            
            if (puDict.ContainsKey(currentPid))
            {
                int uid = puDict[currentPid];
                
                if (userIds.Contains(uid))
                {
                    if (!mapping.ContainsKey(uid))
                    {
                        mapping[uid] = currentPid;
                    }
                }
            }
        }
    }

    return mapping;
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

        public async Task<List<ParticipantUser>> GetByTrainerWithDetailsAsync(int trainerId, DateTime date, int? trainingId = null)
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

        public async Task<List<RefreshAttendanceDto>> GetRefreshPresenceByTrainerAsync(DateTime date, int trainingId, int trainerId)
        {
            return await Task.Run(() =>
            {
                // Ambil semua RecordTraining untuk training ini
                var records = _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(r => r.TrainerId == trainerId
                            && r.TrainingId == trainingId
                            && r.TrainingType == "R"
                            && r.RecordDate.Value.Date == date.Date)
                    .ToList();

                // ✅ Define participantIds dari records
                var participantIds = records
                    .Select(r => r.ParticipantId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();

                // Query participant & user terpisah
                var participants = _uow.Repository<ParticipantUser>()
                    .Table()
                    .Where(p => participantIds.Contains(p.Id))
                    .ToList();

                var userIds = participants
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToList();

                var users = _uow.Repository<User>()
                    .Table()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionary(u => u.Id, u => u);

                var result = participantIds.Select(pid =>
                {
                    var participant = participants.FirstOrDefault(p => p.Id == pid);
                    var user = participant != null && users.ContainsKey(participant.UserId.Value)
                        ? users[participant.UserId.Value]
                        : null;

                    var participantRecords = records.Where(r => r.ParticipantId == pid).ToList();

                    return new RefreshAttendanceDto
                    {
                        UserName     = user?.UserName ?? "-",
                        EmployeeName = user?.EmployeeName ?? "-",
                        Department   = user?.Department ?? "-",
                        TrainingDate = date,
                        TrainingType = "R",
                        HasPreTest   = participantRecords.Any(r => r.StepType == "PreTest"),
                        HasPostTest  = participantRecords.Any(r => r.StepType == "PostTest")
                    };
                })
                .Where(r => r.IsPresent)
                .OrderBy(r => r.EmployeeName)
                .ToList();

                return result;
            });
        }

        public async Task<List<DateTime>> GetTrainingDatesByTrainerAndTypeAsync(int trainerId, string trainingType)
        {
            return await Task.Run(() =>
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(r => r.RecordDate.HasValue 
                            && r.TrainerId == trainerId
                            && r.TrainingType == trainingType)
                    .Select(r => r.RecordDate.Value.Date)
                    .Distinct()
                    .ToList()
            );
        }

        


    public async Task<List<MasterTraining>> GetScheduledTrainingsByDateAsync(DateTime date)
    {
        var targetDate = date.Date;

        var trainingIds = await Task.Run(() =>
            _uow.Repository<RecordTraining>()
                .Table()
                .Where(r => r.RecordDate.HasValue 
                        && r.RecordDate.Value.Date == targetDate)
                .Select(r => r.TrainingId)
                .Distinct()
                .ToList()
        );

        if (!trainingIds.Any()) return new List<MasterTraining>();

        return await Task.Run(() =>
            _uow.Repository<MasterTraining>()
                .Table()
                .Where(mt => trainingIds.Contains(mt.Id))
                .ToList()
        );
    }

 public async Task<List<ParticipantUser>> GetMissingRefreshParticipantsByDateAsync(DateTime date)
{
    // 1. Ambil SEMUA UserId yang berstatus Refresh ('R') di Tabel User
    var userIdsInRefresh = await Task.Run(() =>
        _uow.Repository<User>()
            .Table()
            .Where(u => u.TrainingType == "R")
            .Select(u => u.Id)
            .ToList()
    );

    if (!userIdsInRefresh.Any())
        return new List<ParticipantUser>();

    // 2. CRITICAL FIX: Ambil ParticipantId PERTAMA (MIN ID) per UserId
    // Karena RecordTraining terhubung ke ParticipantId awal saat upload pertama kali.
    var firstParticipantMap = await Task.Run(() =>
        _uow.Repository<ParticipantUser>()
            .Table()
            .Where(pu => pu.UserId.HasValue && userIdsInRefresh.Contains(pu.UserId.Value))
            .OrderBy(pu => pu.Id) // Ascending: Ambil ID terkecil (pertama)
            .ToList()
            .GroupBy(pu => pu.UserId.Value)
            .Select(g => g.First())
            .ToDictionary(k => k.UserId.Value, v => v.Id)
    );

    var firstParticipantIds = firstParticipantMap.Values.ToList();

    if (!firstParticipantIds.Any())
        return new List<ParticipantUser>();

    // 3. Cek di RecordTraining: Siapa saja dari ParticipantId LAMA ini yang SUDAH punya record PostTest Type R?
    var completedParticipantIds = await Task.Run(() =>
        _uow.Repository<RecordTraining>()
            .Table()
            .Where(r => r.StepType == "PostTest"
                     && r.TrainingType == "R"
                     && r.ParticipantId.HasValue
                     && firstParticipantIds.Contains(r.ParticipantId.Value))
            .Select(r => r.ParticipantId.Value)
            .Distinct()
            .ToList()
    );

    // 4. Cari UserId mana yang ParticipantId lamanya BELUM punya record PostTest
    // Kita reverse lookup dari completedParticipantIds ke UserId menggunakan dictionary tadi
    var completedUserIds = new HashSet<int>();
    foreach (var pid in completedParticipantIds)
    {
        var userId = firstParticipantMap.FirstOrDefault(x => x.Value == pid).Key;
        if (userId != 0)
        {
            completedUserIds.Add(userId);
        }
    }

    // 5. Selisih: Yang Berstatus R TAPI UserId-nya tidak ada di list Completed
    var missingUserIds = userIdsInRefresh.Except(completedUserIds).ToList();

    if (!missingUserIds.Any())
        return new List<ParticipantUser>();

    // 6. Ambil Data ParticipantUser TERBARU per UserId yang Missing (untuk tampilan UI/Excel yang rapi)
    // Kita butuh data departemen/trainer terbaru, jadi kita join ke ParticipantUser lagi
    var rawParticipants = await Task.Run(() =>
        _uow.Repository<ParticipantUser>()
            .Table()
            .Where(pu => pu.UserId.HasValue && missingUserIds.Contains(pu.UserId.Value))
            .OrderByDescending(pu => pu.Id) // Ambil yang paling baru untuk data display
            .ToList()
            .GroupBy(pu => pu.UserId.Value)
            .Select(g => g.First()) // Distinct by UserId
            .ToList()
    );

    var userIdsToLoad = rawParticipants.Select(p => p.UserId.Value).Distinct().ToList();
    
    if (!userIdsToLoad.Any()) return rawParticipants;

    var users = await _uow.Repository<User>().GetAllAsync(u => userIdsToLoad.Contains(u.Id));
    var userDict = users.ToDictionary(u => u.Id, u => u);

    foreach (var p in rawParticipants)
    {
        if (userDict.TryGetValue(p.UserId.Value, out var user))
        {
            p.User = user;
        }
    }

    return rawParticipants;
}


    public async Task<List<ParticipantUser>> GetInductionPresenceAsync(DateTime date)
    {
        var targetDate = date.Date;
        Console.WriteLine($"[DEBUG] Start GetInductionPresenceAsync for Date: {targetDate}");

        // 1. Ambil UserId yang punya record INDUCTION ('I') di tanggal tersebut
        var userIds = await Task.Run(() =>
            _uow.Repository<RecordTraining>()
                .Table()
                .Join(_uow.Repository<ParticipantUser>().Table(), 
                    rt => rt.ParticipantId, 
                    pu => pu.Id, 
                    (rt, pu) => new { rt.RecordDate, rt.TrainingType, pu.UserId })
                .Where(x => x.RecordDate.HasValue 
                        && x.RecordDate.Value.Date == targetDate
                        && x.TrainingType.Trim() == "I") // FILTER INDUCTION
                .Select(x => x.UserId)
                .Where(uid => uid.HasValue)
                .Select(uid => uid.Value)
                .Distinct()
                .ToList()
        );

        Console.WriteLine($"[DEBUG] Found {userIds.Count} UserIds with Induction activity on {targetDate}");

        if (!userIds.Any())
            return new List<ParticipantUser>();

        // 2. Ambil ParticipantUser TERBARU per UserId tersebut
        var participants = await Task.Run(() =>
            _uow.Repository<ParticipantUser>()
                .Table()
                .Where(pu => pu.UserId.HasValue && userIds.Contains(pu.UserId.Value))
                .OrderByDescending(pu => pu.Id)
                .ToList()
                .GroupBy(pu => pu.UserId.Value)
                .Select(g => g.First())
                .ToList()
        );

        // 3. Load User Details
        var userIdsToLoad = participants.Select(p => p.UserId.Value).Distinct().ToList();
        
        if (userIdsToLoad.Any())
        {
            var users = await _uow.Repository<User>().GetAllAsync(u => userIdsToLoad.Contains(u.Id));
            var userDict = users.ToDictionary(u => u.Id, u => u);

            foreach (var p in participants)
            {
                if (userDict.TryGetValue(p.UserId.Value, out var user))
                {
                    p.User = user;
                    p.TrainingDate = targetDate;
                }
            }
        }

        return participants;
    }

    public async Task<Dictionary<int, int>> GetInductionOriginalParticipantIdsAsync(DateTime date, List<int> userIds)
{
    if (!userIds.Any()) return new Dictionary<int, int>();

    var mapping = new Dictionary<int, int>();

    var records = await Task.Run(() =>
        _uow.Repository<RecordTraining>()
            .Table()
            .Where(r => r.RecordDate.HasValue 
                    && r.RecordDate.Value.Date == date.Date
                    && r.TrainingType == "I")
            .ToList()
    );

    var validParticipantIds = records
        .Where(r => r.ParticipantId.HasValue)
        .Select(r => r.ParticipantId.Value)
        .Distinct()
        .ToList();
    
    if (!validParticipantIds.Any()) return mapping;

    var participants = await Task.Run(() =>
        _uow.Repository<ParticipantUser>()
            .Table()
            .Where(pu => validParticipantIds.Contains(pu.Id))
            .ToList()
    );

    var puDict = new Dictionary<int, int>();
    foreach (var p in participants)
    {
        if (p.UserId.HasValue)
        {
            int uid = p.UserId.Value;
            if (uid > 0)
            {
                puDict[p.Id] = uid;
            }
        }
    }

    foreach (var r in records)
    {
        if (r.ParticipantId.HasValue)
        {
            int currentPid = r.ParticipantId.Value;
            
            if (puDict.TryGetValue(currentPid, out int uid))
            {
                if (userIds.Contains(uid) && !mapping.ContainsKey(uid))
                {
                    mapping[uid] = currentPid;
                }
            }
        }
    }

    return mapping;
    }


    public async Task<List<ParticipantUser>> GetGlobalRefreshPresenceAsync(DateTime date)
    {
        var targetDate = date.Date;
        var userIds = await Task.Run(() =>
            _uow.Repository<RecordTraining>()
                .Table()
                .Join(_uow.Repository<ParticipantUser>().Table(), 
                    rt => rt.ParticipantId, 
                    pu => pu.Id, 
                    (rt, pu) => new { rt.RecordDate, rt.TrainingType, pu.UserId })
                .Where(x => x.RecordDate.HasValue 
                        && x.RecordDate.Value.Date == targetDate
                        && x.TrainingType.Trim() == "R")
                .Select(x => x.UserId)
                .Where(uid => uid.HasValue)
                .Select(uid => uid.Value)
                .Distinct()
                .ToList()
        );

        if (!userIds.Any()) return new List<ParticipantUser>();

        var participants = await Task.Run(() =>
            _uow.Repository<ParticipantUser>()
                .Table()
                .Where(pu => pu.UserId.HasValue && userIds.Contains(pu.UserId.Value))
                .OrderByDescending(pu => pu.Id)
                .ToList()
                .GroupBy(pu => pu.UserId.Value)
                .Select(g => g.First())
                .ToList()
        );

        var userIdsToLoad = participants.Select(p => p.UserId.Value).Distinct().ToList();
        if (userIdsToLoad.Any())
        {
            var users = await _uow.Repository<User>().GetAllAsync(u => userIdsToLoad.Contains(u.Id));
            var userDict = users.ToDictionary(u => u.Id, u => u);
            foreach (var p in participants)
            {
                if (userDict.TryGetValue(p.UserId.Value, out var user))
                {
                    p.User = user;
                    p.TrainingDate = targetDate;
                }
            }
        }
        return participants;
    }

}