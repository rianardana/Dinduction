using System;
using System.Linq;
using System.Threading.Tasks;
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using Dinduction.Application.DTOs;

namespace Dinduction.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _uow;

    public QuestionService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<List<Question>> GetAllWithDetailsAsync()
{
    var questions = await Task.FromResult(_uow.Repository<Question>().Table().ToList());
    
    if (!questions.Any()) return questions;
    
    var trainingIds = questions.Where(q => q.TrainingId.HasValue).Select(q => q.TrainingId.Value).ToList();
    var trainings = await Task.FromResult(_uow.Repository<MasterTraining>().Table().Where(t => trainingIds.Contains(t.Id)).ToList());
    
    
    var sectionIds = trainings.Where(t => t.SectionId.HasValue).Select(t => t.SectionId.Value).ToList();
    var sections = await Task.FromResult(_uow.Repository<Section>().Table().Where(s => sectionIds.Contains(s.Id)).ToList());
    
    
    var sectionDict = sections.ToDictionary(s => s.Id, s => s);
    var trainingDict = trainings.ToDictionary(t => t.Id, t => 
    {
        t.Section = t.SectionId.HasValue && sectionDict.ContainsKey(t.SectionId.Value) 
            ? sectionDict[t.SectionId.Value] 
            : null;
        return t;
    });
    
    
    foreach (var q in questions)
    {
        q.Training = q.TrainingId.HasValue && trainingDict.ContainsKey(q.TrainingId.Value)
            ? trainingDict[q.TrainingId.Value]
            : null;
    }
    
    return questions;
}

    public async Task<Question?> GetByIdAsync(int id)
    {
        return await Task.FromResult(
            _uow.Repository<Question>()
                .Table()
                .FirstOrDefault(q => q.Id == id)
        );
    }

    public async Task InsertAsync(Question obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<Question>().Add(obj);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Question obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        _uow.Repository<Question>().Add(obj);
        await _uow.SaveChangesAsync();
    }

   public async Task DeleteAsync(int id)
    {
        var obj = await Task.FromResult(
            _uow.Repository<Question>()
                .Table()
                .FirstOrDefault(q => q.Id == id)
        );
        
        if (obj != null)
        {
            _uow.Repository<Question>().Delete(obj);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<VQuestionAnswer?> GetQuestionByNumberAsync(int trainingId, int number)
    {
        return await Task.FromResult(
            _uow.Repository<VQuestionAnswer>()
                .Table()
                .FirstOrDefault(c => c.TrainingId == trainingId && c.Number == number)
        );
    }

    public async Task<string?> GetTrueFalseAsync(int questionId)
    {
        return await Task.FromResult(
            _uow.Repository<Answer>()
                .Table()
                .Where(a => a.QuestionId == questionId && a.RightAnswer != null)
                .Select(a => a.RightAnswer)
                .FirstOrDefault()
        );
    }

    public async Task<int> GetTrainingIdAsync(int id)
    {
        var record = await Task.FromResult(
            _uow.Repository<VQuestionAnswer>()
                .Table()
                .FirstOrDefault(c => c.Id == id)
        );
        return record?.TrainingId ?? 0;
    }

    public async Task<int> GetNumberAsync(int id)
    {
        var record = await Task.FromResult(
            _uow.Repository<VQuestionAnswer>()
                .Table()
                .FirstOrDefault(c => c.Id == id)
        );
        return record?.Number ?? 0;
    }

    public async Task<int> GetLastNumberAsync()
    {
        var last = await Task.FromResult(
            _uow.Repository<Question>()
                .Table()
                .OrderByDescending(c => c.Number)
                .Select(c => c.Number)
                .FirstOrDefault()
        );
        return (last ?? 0) + 1;
    }

    public async Task<List<VQuestionAnswerUser>> GetListAnswerAsync(int trainingId, int participantId)
    {
        return await Task.FromResult(
            _uow.Repository<VQuestionAnswerUser>()
                .Table()
                .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId)
                .GroupBy(c => c.QuestionTraining)
                .Select(g => g.OrderByDescending(c => c.QuizNumber).FirstOrDefault())
                .OrderBy(c => c.Number)
                .ToList()
        );
    }

    public async Task<List<VQuestionAnswerUser>> GetListAnswerHistoryAsync(int trainingId, int participantId, int quizNumber)
    {
        return await Task.FromResult(
            _uow.Repository<VQuestionAnswerUser>()
                .Table()
                .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId && c.QuizNumber == quizNumber)
                .GroupBy(c => c.QuestionTraining)
                .Select(g => g.FirstOrDefault())
                .OrderBy(c => c.Number)
                .ToList()
        );
    }

    public async Task<List<VQuestionAnswerUser>> GetLastListAnswerAsync(int trainingId, int participantId)
    {
        var latestQuizNumber = await Task.FromResult(
            _uow.Repository<VQuestionAnswerUser>()
                .Table()
                .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId)
                .OrderByDescending(c => c.QuizNumber)
                .Select(c => c.QuizNumber)
                .FirstOrDefault()
        );

        return await Task.FromResult(
            _uow.Repository<VQuestionAnswerUser>()
                .Table()
                .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId && c.QuizNumber == latestQuizNumber)
                .OrderBy(c => c.Number)
                .ToList()
        );
    }

    public async Task<VMasterQuestion?> GetDetailAsync(int trainingId, int participantId)
    {
        return await Task.FromResult(
            _uow.Repository<VMasterQuestion>()
                .Table()
                .Where(c => c.TrainingId == trainingId && c.ParticipantId == participantId)
                .FirstOrDefault()
        );
    }

    public async Task<int> GetTotalQuestionAsync(int trainingId)
    {
        return await Task.FromResult(
            _uow.Repository<Question>()
                .Table()
                .Count(c => c.TrainingId == trainingId)
        );
    }
}