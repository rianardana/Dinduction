// RecordTrainingService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using Dinduction.Application.DTOs;
using Dinduction.Application.Models;
using Dinduction.Application.Services;

namespace Dinduction.Infrastructure.Services
{
    public class RecordTrainingService : IRecordTrainingService
    {
        private readonly IUnitOfWork _uow;

        public RecordTrainingService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<List<RecordTraining>> GetAllAsync()
        {
            return await Task.FromResult(_uow.Repository<RecordTraining>().Table().ToList());
        }

        public async Task InsertAsync(RecordTraining record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            _uow.Repository<RecordTraining>().Add(record);
            await _uow.SaveChangesAsync();
        }

        public async Task<bool> IsAnsweredAsync(int participantId, int questionId)
        {
            return await Task.FromResult(
                _uow.Repository<RecordTraining>().Table()
                    .Any(c => c.ParticipantId == participantId && c.QuestionId == questionId && c.UserAnswer != null)
            );
        }

    
        public async Task<List<int>> GetAnswerAsync(int participantId, int trainingId, int quizNo)
        {
            return await Task.FromResult(
                _uow.Repository<RecordTraining>().Table()
                    .Where(c => c.ParticipantId == participantId && c.TrainingId == trainingId && c.QuizNumber == quizNo)
                    .Select(c => c.NumberQuestion.Value)
                    .ToList()
            );
        }

        public async Task<bool> CheckAnsweredAsync(int participantId, int trainingId, int quizNo, int numberQuestion)
        {
            return await Task.FromResult(
                _uow.Repository<RecordTraining>().Table()
                    .Any(c => c.ParticipantId == participantId && c.TrainingId == trainingId && c.QuizNumber == quizNo && c.NumberQuestion == numberQuestion)
            );
        }

        public async Task<VRecordTraining> GetByParticipantIdAsync(int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordTraining>().Table().FirstOrDefault(c => c.ParticipantId == participantId)
            );
        }

        public async Task<bool> CheckExistingAsync(int trainingId, int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<RecordTraining>().Table()
                    .Any(c => c.TrainingId == trainingId && c.ParticipantId == participantId)
            );
        }

        public async Task<bool> CheckFailedAsync(int trainingId, int participantId, int quizNo)
        {
            var totalQuestions = await Task.FromResult(
                _uow.Repository<RecordTraining>().Table()
                    .Count(c => c.TrainingId == trainingId && c.QuizNumber == quizNo)
            );

            var correctAnswers = await Task.FromResult(
                _uow.Repository<RecordTraining>().Table()
                    .Count(c => c.TrainingId == trainingId && c.ParticipantId == participantId && c.QuizNumber == quizNo && c.IsTrue == true)
            );

            var minCorrect = (int)Math.Ceiling(totalQuestions * 0.8);
            return correctAnswers >= minCorrect;
        }

        public async Task<int> GetTrainerAsync(int participantId, int trainingId)
        {
            var record = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .FirstOrDefault(c => c.ParticipantId == participantId && c.TrainingId == trainingId)
            );
            return record?.TrainerId ?? 0;
        }

        public async Task<bool> HasIncompleteQuizAsync(int trainingId, int participantId, int quizNo)
        {
            var totalQuestion = await Task.FromResult(
                _uow.Repository<Question>()
                    .Table()
                    .Count(q => q.TrainingId == trainingId)
            );
            
            var totalAnswer = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Count(r => r.TrainingId == trainingId &&
                            r.ParticipantId == participantId &&
                            r.QuizNumber == quizNo)
            );
            
            return totalAnswer < totalQuestion;
        }

        public async Task<int> GetLastQuizNoAsync(int participantId, int trainingId)
        {
            // Ambil semua QuizNumber ke client
            var quizNumbers = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(r => r.TrainingId == trainingId && r.ParticipantId == participantId)
                    .Select(r => r.QuizNumber)
                    .ToList() // ✅ Ini force client evaluation
            );

            // Proses di client
            if (quizNumbers.Count == 0)
                return 0;
                
            return quizNumbers.Max() ?? 0;
        }

        public async Task<bool> CheckPassedPreviousAsync(int trainingId, int participantId, int prevQuizNo)
        {
            if (prevQuizNo <= 0) return false;

            var totalQuestion = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Count(r => r.TrainingId == trainingId &&
                            r.ParticipantId == participantId &&
                            r.QuizNumber == prevQuizNo)
            );

            if (totalQuestion == 0) return false;

            var totalTrue = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Count(r => r.TrainingId == trainingId &&
                            r.ParticipantId == participantId &&
                            r.QuizNumber == prevQuizNo &&
                            r.IsTrue == true)
            );

            var nilai = (totalTrue / (double)totalQuestion) * 100;
            return nilai >= 80;
        }

        public async Task<QuizStatus> GetQuizStatusAsync(int trainingId, int participantId)
        {
            var totalQuestions = await Task.FromResult(
                _uow.Repository<Question>()
                    .Table()
                    .Count(q => q.TrainingId == trainingId)
            );

            var quizRecords = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(r => r.TrainingId == trainingId && r.ParticipantId == participantId)
                    .ToList()
            );

            if (quizRecords.Count == 0)
                return QuizStatus.Start;

            var quizGroups = quizRecords.GroupBy(r => r.QuizNumber).OrderBy(g => g.Key);
            var lastQuizGroup = quizGroups.Last();
            var lastQuizNumber = lastQuizGroup.Key;
            var answered = lastQuizGroup.Count();
            var correct = lastQuizGroup.Count(a => a.IsTrue == true);

            if (answered < totalQuestions)
                return QuizStatus.Continue;

            // ✅ Hitung persentase BENAR
            double percentageCorrect = ((double)correct / totalQuestions) * 100;

            if (percentageCorrect >= 80)
            {
                // Lulus
                return QuizStatus.Done;
            }
            else
            {
                // Gagal
                if (lastQuizNumber == 1)
                {
                    return QuizStatus.Second;
                }
                else
                {
                    return QuizStatus.DoneFailed;
                }
            }
        }

        public async Task<int> GetFirstSecondAsync(int trainingId, int participantId)
        {
            var result = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Count(c => c.TrainingId == trainingId && c.ParticipantId == participantId && c.QuizNumber == 1)
            );
            
            var resultNew = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Count(c => c.TrainingId == trainingId && c.ParticipantId == participantId && c.QuizNumber == 2)
            );
            
            if (resultNew >= 5)
            {
                return 3;
            }
            else if (result >= 5)
            {
                return 2;
            }
            return 1;
        }

        public async Task<List<VRecordTraining>> GetOwnResultAsync(int participantId)
        {
            var result = await Task.FromResult(
                _uow.Repository<VRecordTraining>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .ToList()
            );

            var uniqueResults = result
                .GroupBy(r => r.TrainingId)
                .Select(group => group.First())
                .ToList();

            return uniqueResults;
        }

        public async Task<List<RecordTraining>> GetRecapAsync(int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .GroupBy(c => new { c.ParticipantId, c.TrainingId, c.QuizNumber })
                    .Select(g => g.FirstOrDefault())
                    .OrderBy(r => r.QuizNumber)
                    .ThenBy(r => r.TrainingId)
                    .ThenBy(r => r.ParticipantId)
                    .ToList()
            );
        }

        public async Task<VRecordResult> GetResultAsync(int trainingId, int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordResult>()
                    .Table()
                    .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId)
                    .OrderByDescending(c => c.QuizNumber)
                    .FirstOrDefault()
            );
        }

        public async Task<VRecordResult> GetResultHistoryAsync(int trainingId, int participantId, int quizNumber)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordResult>()
                    .Table()
                    .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId && c.QuizNumber == quizNumber)
                    .FirstOrDefault()
            );
        }

        public async Task<List<VRecordResult>> GetRecapResultAsync(int trainingId, int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordResult>()
                    .Table()
                    .Where(c => c.TrainingId.HasValue && c.TrainingId.Value == trainingId && c.ParticipantId == participantId)
                    .ToList()
            );
        }

        public async Task<int> GetScoreAsync(int trainingId, int participantId)
        {
            var latestQuizNumber = await Task.FromResult(
                _uow.Repository<VQuestionAnswerUser>()
                    .Table()
                    .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId)
                    .OrderByDescending(c => c.QuizNumber)
                    .Select(c => c.QuizNumber)
                    .FirstOrDefault()
            );

            int totalQuestions = await Task.FromResult(
                _uow.Repository<Question>()
                    .Table()
                    .Count(q => q.TrainingId == trainingId)
            );

            if (totalQuestions == 0) return 0;

            int correctAnswer = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Count(c => c.TrainingId == trainingId &&
                            c.ParticipantId == participantId &&
                            c.QuizNumber == latestQuizNumber &&
                            c.IsTrue == true)
            );

            double score = (double)correctAnswer / totalQuestions * 100.0;
            return (int)Math.Round(score);
        }

        public async Task<int> GetScoreHistoryAsync(int trainingId, int participantId, int quizNumber)
        {
            int totalQuestions = await Task.FromResult(
                _uow.Repository<Question>()
                    .Table()
                    .Count(q => q.TrainingId == trainingId)
            );

            if (totalQuestions == 0) return 0;

            int correctAnswer = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Count(c => c.TrainingId == trainingId &&
                            c.ParticipantId == participantId &&
                            c.QuizNumber == quizNumber &&
                            c.IsTrue == true)
            );

            double score = ((double)correctAnswer / totalQuestions) * 100;
            return (int)Math.Round(score);
        }

        public async Task<List<RecordTraining>> GetAllScoresAsync(int participantId)
        {
            var scores = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId && c.IsTrue == true)
                    .GroupBy(c => c.TrainingId)
                    .Select(g => new RecordTraining
                    {
                        TrainingId = g.Key,
                        Score = g.Count() * 20
                    })
                    .ToList()
            );

            return scores;
        }

        public async Task<List<VRecordMaster>> GetLatestResultByParticipantAsync(int participantId)
        {
            var query = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .GroupBy(c => c.TrainingName)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );
            return query;
        }

        public async Task<List<VRecordMaster>> GetLatestResultByTrainerAsync(int trainerId)
        {
            var query = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.TrainerId == trainerId)
                    .GroupBy(c => c.ParticipantId)
                    .Select(g => g.OrderByDescending(r => r.RecordDate).FirstOrDefault())
                    .OrderByDescending(c => c.RecordDate)
                    .ThenBy(c => c.EmployeeName)
                    .ToList()
            );
            return query;
        }

        public async Task<List<VRecordMaster>> GetResultByIdAsync(int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .ToList()
            );
        }
        
        public async Task<List<VRecordMaster>> GetResultByParticipantAndTrainingAsync(int participantId, int trainingId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId && c.TrainingId == trainingId)
                    .ToList()
            );
        }

        public async Task<int> GetLastScoreAsync(int trainingId, int participantId)
        {
            var latestQuizNumber = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId)
                    .OrderByDescending(c => c.QuizNumber)
                    .Select(c => c.QuizNumber)
                    .FirstOrDefault()
            );

            int totalQuestions = await Task.FromResult(
                _uow.Repository<Question>()
                    .Table()
                    .Count(q => q.TrainingId == trainingId)
            );

            if (totalQuestions == 0) return 0;

            int correctAnswers = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Count(c => c.TrainingId == trainingId &&
                            c.ParticipantId == participantId &&
                            c.QuizNumber == latestQuizNumber &&
                            c.IsTrue == true)
            );

            double score = (double)correctAnswers / totalQuestions * 100.0;
            return (int)Math.Round(score);
        }

        public async Task<List<VRecordMaster>> GetLastResultByIdAsync(int participantId, int trainerId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId && c.TrainerId == trainerId)
                    .GroupBy(c => c.TrainingName)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );
        }

        public async Task<int> CountCompletedAsync(int participantId, int trainerId)
        {
            var query = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId && c.TrainerId == trainerId)
                    .GroupBy(c => c.TrainingName)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );
            return query.Count;
        }

        public async Task<int> CountCompletedForAdminAsync(int participantId)
        {
            var query = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .GroupBy(c => c.TrainingName)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );
            return query.Count;
        }

        public async Task<int> CountFailedAsync(int participantId, int trainerId)
        {
            var allData = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId && c.TrainerId == trainerId)
                    .OrderBy(c => c.TrainingId)
                    .ThenBy(c => c.QuizNumber)
                    .ToList()
            );

            var groups = allData.GroupBy(c => c.TrainingId);
            int failedCount = 0;

            foreach (var group in groups)
            {
                var lastQuizNumber = group.Max(x => x.QuizNumber);
                var lastAttemptQuestions = group.Where(c => c.QuizNumber == lastQuizNumber).ToList();

                if (!lastAttemptQuestions.Any()) continue;

                int totalSoal = lastAttemptQuestions.Count;
                int totalBenar = lastAttemptQuestions.Count(c => c.IsTrue == true);
                double score = (totalSoal > 0) ? ((double)totalBenar / totalSoal) * 100 : 0;

                if (score < 80)
                {
                    failedCount++;
                }
            }

            return failedCount;
        }

        public async Task<int> CountFailedForAdminAsync(int participantId)
        {
            var allData = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .OrderBy(c => c.TrainingId)
                    .ThenBy(c => c.QuizNumber)
                    .ToList()
            );

            var groups = allData.GroupBy(c => c.TrainingId);
            int failedCount = 0;

            foreach (var group in groups)
            {
                var lastQuizNumber = group.Max(x => x.QuizNumber);
                var lastAttemptQuestions = group.Where(c => c.QuizNumber == lastQuizNumber).ToList();

                if (!lastAttemptQuestions.Any()) continue;

                int totalSoal = lastAttemptQuestions.Count;
                int totalBenar = lastAttemptQuestions.Count(c => c.IsTrue == true);
                double score = (totalSoal > 0) ? ((double)totalBenar / totalSoal) * 100 : 0;

                if (score < 80)
                {
                    failedCount++;
                }
            }

            return failedCount;
        }

        public async Task<List<VResult>> GetAllResultsAsync(string? searchBy = null)
        {
            var groupedQuery = await Task.FromResult(
                _uow.Repository<VResult>()
                    .Table()
                    .GroupBy(c => new { c.ParticipantId, c.TrainingId })
                    .ToList()
            );

            var query = groupedQuery
                .Select(g => new VResult
                {
                    ParticipantId = g.Key.ParticipantId,
                    TrainingId = g.Key.TrainingId,
                    EmployeeName = g.FirstOrDefault()?.EmployeeName,
                    TrainingName = g.FirstOrDefault()?.TrainingName,
                    UserName = g.FirstOrDefault()?.UserName,
                    RecordDate = g.FirstOrDefault()?.RecordDate,
                    Score = g.Count() == 0 ? 0 : (int)Math.Round((double)g.Count(x => x.IsTrue == true) / g.Count() * 100)
                });

            if (!string.IsNullOrEmpty(searchBy))
            {
                var searchTerm = searchBy.ToLower();
                query = query.Where(c =>
                    (c.EmployeeName?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.UserName?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.TrainingName?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.RecordDate?.ToString().Contains(searchTerm) ?? false) ||
                    c.Score.ToString().Contains(searchTerm)
                );
            }

            return await Task.FromResult(query.OrderBy(c => c.ParticipantId).ToList());
        }

        public async Task<List<VRecordMaster>> GetLatestResultsForAdminAsync()
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .GroupBy(c => c.ParticipantId)
                    .Select(g => g.OrderByDescending(r => r.RecordDate).FirstOrDefault())
                    .OrderByDescending(c => c.RecordDate)
                    .ThenBy(c => c.EmployeeName)
                    .ToList()
            );
        }

        public async Task<List<VRecordMaster>> GetFailedAttemptsAsync()
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.QuizNumber == 1)
                    .GroupBy(c => new { c.ParticipantId, c.TrainingId })
                    .Where(g => g.Count(r => !r.IsTrue.GetValueOrDefault()) >= 2)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );
        }

       

        public async Task<List<VRecordMaster>> GetAuditRecordsAsync()
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .GroupBy(c => c.ParticipantId)
                    .Select(g => g.FirstOrDefault())
                    .ToList()
            );
        }

        public async Task<List<VRecordMaster>> GetLastResultForAdminAsync(int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .GroupBy(c => c.TrainingName)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );
        }

        public async Task<List<VRecordMaster>> GetHistoryAsync(int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .GroupBy(c => new { c.TrainingName, c.QuizNumber })
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );
        }

        public async Task<int> GetSuccessAsync(int participantId)
        {
            var successCount = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(x => x.ParticipantId == participantId)
                    .GroupBy(x => x.TrainingId)
                    .Select(g => new
                    {
                        MaxQuiz = g.OrderByDescending(x => x.QuizNumber).FirstOrDefault(),
                        TrueCount = g.Count(x => x.IsTrue == true)
                    })
                    .Count(result => 
                        (result.MaxQuiz != null && result.MaxQuiz.IsTrue == true) || 
                        result.TrueCount >= 4)
            );
            return successCount;
        }

        public async Task<int> GetFailedAsync(int participantId)
        {
            var failedCount = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(x => x.ParticipantId == participantId)
                    .GroupBy(x => x.TrainingId)
                    .Select(g => new
                    {
                        MaxQuiz = g.OrderByDescending(x => x.QuizNumber).FirstOrDefault(),
                        TrueCount = g.Count(x => x.IsTrue == true)
                    })
                    .Count(result => 
                        (result.MaxQuiz != null && result.MaxQuiz.IsTrue == false && result.TrueCount < 4))
            );
            return failedCount;
        }

        public async Task<bool> DeleteRecordAsync(int participantId, int trainingId, int quizNumber)
        {
            var records = await Task.FromResult(
                _uow.Repository<RecordTraining>()
                    .Table()
                    .Where(r => r.ParticipantId == participantId &&
                            r.TrainingId == trainingId &&
                            r.QuizNumber == quizNumber)
                    .ToList()
            );

            if (records.Any())
            {
                foreach (var record in records)
                {
                    _uow.Repository<RecordTraining>().Delete(record);
                }
                await _uow.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<UserTrainingDTO>> GetLatestResultForUserAsync(int participantId)
        {
            var allTrainings = await Task.FromResult(_uow.Repository<MasterTraining>().Table().ToList());
            var results = new List<UserTrainingDTO>();

            foreach (var training in allTrainings)
            {
                var records = await Task.FromResult(
                    _uow.Repository<VRecordMaster>()
                        .Table()
                        .Where(r => r.ParticipantId == participantId && r.TrainingId == training.Id)
                        .ToList()
                );

                bool? isPass = null;

                if (records.Count > 0)
                {
                    int latestQuiz = records.Max(r => r.QuizNumber.GetValueOrDefault());
                    var latestRecords = records.Where(r => r.QuizNumber == latestQuiz).ToList();

                    int totalQuestions = latestRecords.Count;
                    int correctAnswers = latestRecords.Count(r => r.IsTrue.GetValueOrDefault());

                    double score = (double)correctAnswers / totalQuestions * 100.0;
                    isPass = score >= 80;
                }

                results.Add(new UserTrainingDTO
                {
                    TrainingId = training.Id,
                    TrainingName = training.TrainingName,
                    IsPass = isPass
                });
            }

            return results;
        }

        public async Task<RecordTraining> GetLastRecordByParticipantAsync(int participantId)
        {
            return await Task.FromResult(
                _uow.Repository<RecordTraining>().Table().Where(r => r.ParticipantId == participantId)
                    .OrderByDescending(r => r.RecordDate)
                    .FirstOrDefault()
            );
        }

        public async Task<(List<VRecordMaster> Data, int TotalCount)> SearchResultAsync(DataTableAjaxPostModel model, int participantId)
        {
            var searchBy = model.search?.value;

            var query = await Task.Run(() =>
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => c.ParticipantId == participantId)
                    .GroupBy(c => c.TrainingName)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList()
            );

            var totalCount = query.Count;

            return (query, totalCount);
        }

    public async Task<(List<VRecordMaster> Data, int TotalCount)> SearchByTrainerAsync(DataTableAjaxPostModel model, int trainerId)
    {
        return await Task.Run(() =>
        {
            var allRecords = _uow.Repository<VRecordMaster>()
                .Table()        
                .Where(c => c.TrainerId == trainerId)
                .ToList();

            var query = allRecords
                .GroupBy(c => c.ParticipantId)
                .Select(g => g.OrderByDescending(r => r.RecordDate).FirstOrDefault())
                .Where(x => x != null)
                .OrderByDescending(c => c.RecordDate)
                .ThenBy(c => c.EmployeeName)
                .ToList();

            var totalCount = query.Count;
            return (query, totalCount);
        });
    }


    public async Task<(List<VRecordMaster> Data, int TotalCount)> SearchForAdminAsync(DataTableAjaxPostModel model)
    {
        var searchBy = model.search?.value;

        var query = await Task.Run(() =>
            _uow.Repository<VRecordMaster>()
                .Table()
                .GroupBy(c => c.ParticipantId)
                .Select(g => g.OrderByDescending(r => r.RecordDate).FirstOrDefault())
                .OrderByDescending(c => c.RecordDate)
                .ThenBy(c => c.EmployeeName)
                .ToList()
        );

        var totalCount = query.Count;

        return (query, totalCount);
    }


    public async Task<(List<VRecordMaster> Data, int TotalCount)> SearchFailedAsync(DataTableAjaxPostModel model)
    {
        var searchBy = model.search?.value;

        var query = await Task.Run(() =>
            _uow.Repository<VRecordMaster>()
                .Table()
                .Where(c => c.QuizNumber == 1)
                .GroupBy(c => new { c.ParticipantId, c.TrainingId })
                .Where(g => g.Count(r => !r.IsTrue.Value) >= 2)
                .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                .ToList()
        );

        var totalCount = query.Count;

        return (query, totalCount);
    }


    public async Task<(List<VRecordMaster> Data, int TotalCount)> SearchForAuditAsync(DataTableAjaxPostModel model)
    {
        var searchBy = model.search?.value;

        var query = await Task.Run(() =>
            _uow.Repository<VRecordMaster>()
                .Table()
                .GroupBy(c => c.ParticipantId)
                .Select(g => g.FirstOrDefault())
                .ToList()
        );

        var totalCount = query.Count;

        return (query, totalCount);
    }

    public async Task<(IPagedList<VResult> Data, int TotalCount)> SearchRecordAsync(DataTableAjaxPostModel model)
    {
        var searchBy = model.search?.value;

        var groupedQuery = _uow.Repository<VResult>().Table()
            .GroupBy(c => new { c.ParticipantId, c.TrainingId });

        var totalCount = await Task.Run(() => groupedQuery.Count());

        var query = await Task.Run(() =>
            groupedQuery
                .AsEnumerable()
                .Select(g => new VResult
                {
                    ParticipantId = g.Key.ParticipantId,
                    TrainingId = g.Key.TrainingId,
                    EmployeeName = g.FirstOrDefault().EmployeeName,
                    TrainingName = g.FirstOrDefault().TrainingName,
                    UserName = g.FirstOrDefault().UserName,
                    RecordDate = g.FirstOrDefault().RecordDate,
                    Score = g.Count() == 0 
                        ? 0 
                        : (int)Math.Round((double)g.Count(x => x.IsTrue == true) / g.Count() * 100)
                })
        );

        // Filter berdasarkan search
        if (!string.IsNullOrEmpty(searchBy))
        {
            query = query.Where(c =>
                c.EmployeeName?.ToLower().Contains(searchBy.ToLower()) == true ||
                c.UserName?.ToLower().Contains(searchBy.ToLower()) == true ||
                c.TrainingName?.ToLower().Contains(searchBy.ToLower()) == true ||
                c.RecordDate.ToString().Contains(searchBy) ||
                c.Score.ToString().Contains(searchBy)
            ).OrderBy(c => c.ParticipantId);
        }
        else
        {
            query = query.OrderBy(c => c.ParticipantId);
        }

        // Pagination
        var pagedData = new PagedList<VResult>(query.ToList(), model.start, model.length);

        return (pagedData, totalCount);
    }

    public async Task<IEnumerable<VRecordMaster>> GetAllChartAsync(DateTime dateStart, DateTime dateEnd)
    {
        return await Task.Run(() =>
            _uow.Repository<VRecordMaster>()
                .Table()
                .Where(c => c.RecordDate >= dateStart && c.RecordDate <= dateEnd)
                .AsEnumerable()
        );
    }

        // ✅ BATCH VERSION - CountCompleted
        public async Task<Dictionary<int, int>> CountCompletedBatchAsync(List<int> participantIds, int trainerId)
        {
            var allData = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => participantIds.Contains(c.ParticipantId.Value) && c.TrainerId == trainerId)
                    .ToList()
            );

            var result = new Dictionary<int, int>();

            foreach (var participantId in participantIds)
            {
                var participantData = allData
                    .Where(c => c.ParticipantId == participantId)
                    .GroupBy(c => c.TrainingName)
                    .Select(g => g.OrderByDescending(r => r.Id).FirstOrDefault())
                    .ToList();

                result[participantId] = participantData.Count;
            }

            return result;
        }

        // ✅ BATCH VERSION - CountFailed
        public async Task<Dictionary<int, int>> CountFailedBatchAsync(List<int> participantIds, int trainerId)
        {
            var allData = await Task.FromResult(
                _uow.Repository<VRecordMaster>()
                    .Table()
                    .Where(c => participantIds.Contains(c.ParticipantId.Value) && c.TrainerId == trainerId)
                    .OrderBy(c => c.TrainingId)
                    .ThenBy(c => c.QuizNumber)
                    .ToList()
            );

            var result = new Dictionary<int, int>();

            foreach (var participantId in participantIds)
            {
                var participantData = allData.Where(c => c.ParticipantId == participantId).ToList();
                var groups = participantData.GroupBy(c => c.TrainingId);
                int failedCount = 0;

                foreach (var group in groups)
                {
                    var lastQuizNumber = group.Max(x => x.QuizNumber);
                    var lastAttemptQuestions = group.Where(c => c.QuizNumber == lastQuizNumber).ToList();

                    if (!lastAttemptQuestions.Any()) continue;

                    int totalSoal = lastAttemptQuestions.Count;
                    int totalBenar = lastAttemptQuestions.Count(c => c.IsTrue == true);
                    double score = (totalSoal > 0) ? ((double)totalBenar / totalSoal) * 100 : 0;

                    if (score < 80)
                    {
                        failedCount++;
                    }
                }

                result[participantId] = failedCount;
            }

            return result;
        }

    }
}