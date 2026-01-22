
using System.Collections.Generic;
using System.Threading.Tasks;
using Dinduction.Domain.Entities;
using Dinduction.Application.DTOs;

namespace Dinduction.Application.Interfaces;

public interface IQuestionService
{
    Task<List<Question>> GetAllWithDetailsAsync();
    Task<Question?> GetByIdAsync(int id);
    Task InsertAsync(Question obj);
    Task UpdateAsync(Question obj);
    Task DeleteAsync(int id);
    Task<VQuestionAnswer?> GetQuestionByNumberAsync(int trainingId, int number);
    Task<List<VQuestionAnswerUser>> GetListAnswerAsync(int trainingId, int participantId);
    Task<List<VQuestionAnswerUser>> GetListAnswerHistoryAsync(int trainingId, int participantId, int quizNumber);
    Task<VMasterQuestion?> GetDetailAsync(int trainingId, int participantId);
    Task<int> GetTrainingIdAsync(int id);
    Task<int> GetNumberAsync(int id);
    Task<int> GetLastNumberAsync();
    Task<string?> GetTrueFalseAsync(int questionId);
    Task<List<VQuestionAnswerUser>> GetLastListAnswerAsync(int trainingId, int participantId);
    Task<int> GetTotalQuestionAsync(int trainingId);
}