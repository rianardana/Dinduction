
using System.Collections.Generic;
using System.Threading.Tasks;
using Dinduction.Domain.Entities;
using Dinduction.Application.DTOs;

namespace Dinduction.Application.Interfaces
{
    public interface IRecordTrainingService
    {
        Task<List<RecordTraining>> GetAllAsync();
        Task InsertAsync(RecordTraining record);
        Task<bool> IsAnsweredAsync(int participantId, int questionId);
        Task<int> GetTrainerAsync(int participantId, int trainingId);
        Task<List<int>> GetAnswerAsync(int participantId, int trainingId, int quizNo);
        Task<bool> CheckAnsweredAsync(int participantId, int trainingId, int quizNo, int numberQuestion);
        Task<VRecordTraining> GetByParticipantIdAsync(int participantId);
        Task<bool> CheckExistingAsync(int trainingId, int participantId);
        Task<bool> CheckFailedAsync(int trainingId, int participantId, int quizNo);
        Task<VRecordResult> GetResultAsync(int trainingId, int participantId);
        Task<VRecordResult> GetResultHistoryAsync(int trainingId, int participantId, int quizNumber);
        Task<int> GetScoreAsync(int trainingId, int participantId);
        Task<int> GetScoreHistoryAsync(int trainingId, int participantId, int quizNumber);
        Task<int> GetFirstSecondAsync(int trainingId, int participantId);
        Task<List<VRecordTraining>> GetOwnResultAsync(int participantId);
        Task<List<RecordTraining>> GetRecapAsync(int participantId);
        Task<List<RecordTraining>> GetAllScoresAsync(int participantId);
        Task<List<VRecordResult>> GetRecapResultAsync(int trainingId, int participantId);
        Task<int> GetLastScoreAsync(int trainingId, int participantId);
        Task<List<VRecordMaster>> GetResultByIdAsync(int participantId);
        Task<List<VRecordMaster>> GetResultByParticipantAndTrainingAsync(int participantId, int trainingId);
        Task<List<VRecordMaster>> GetLastResultByIdAsync(int participantId, int trainerId);
        Task<int> CountCompletedAsync(int participantId, int trainerId);
        Task<int> CountCompletedForAdminAsync(int participantId);
        Task<int> CountFailedAsync(int participantId, int trainerId);
        Task<int> CountFailedForAdminAsync(int participantId);
        Task<int> GetSuccessAsync(int participantId);
        Task<int> GetFailedAsync(int participantId);
        Task<bool> DeleteRecordAsync(int participantId, int trainingId, int quizNumber);
        Task<List<UserTrainingDTO>> GetLatestResultForUserAsync(int participantId);
        Task<bool> HasIncompleteQuizAsync(int trainingId, int participantId, int quizNo);
        Task<int> GetLastQuizNoAsync(int participantId, int trainingId);
        Task<QuizStatus> GetQuizStatusAsync(int trainingId, int participantId);
        Task<bool> CheckPassedPreviousAsync(int trainingId, int participantId, int prevQuizNo);
        Task<List<VRecordMaster>> GetLatestResultByParticipantAsync(int participantId);
        Task<RecordTraining> GetLastRecordByParticipantAsync(int participantId);
    }
}