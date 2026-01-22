// MasterTrainingVM.cs
using Dinduction.Domain.Entities;

namespace Dinduction.Web.Models;

public class MasterTrainingVM
{
    // Properti dari MasterTraining
    public int Id { get; set; }
    public string? TrainingName { get; set; }
    public string? EvaluationForm { get; set; }
    public DateOnly? FormDateRegistration { get; set; }
    public string? FormNumberRegistration { get; set; }
    public string? Purpose1 { get; set; }
    public string? PurposeEnglish1 { get; set; }
    public string? Purpose2 { get; set; }
    public string? PurposeEnglish2 { get; set; }
    
    // Properti tambahan untuk Quiz
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }
    public int QuizNo { get; set; }
    public DateTime? TrainingDate { get; set; }
    public bool IsActive { get; set; }

    // ✅ Method static HARUS di dalam class
    public static MasterTrainingVM FromEntity(MasterTraining entity)
    {
        return new MasterTrainingVM
        {
            Id = entity.Id,
            TrainingName = entity.TrainingName,
            EvaluationForm = entity.EvaluationForm,
            FormDateRegistration = entity.FormDateRegistration,
            FormNumberRegistration = entity.FormNumberRegistration,
            Purpose1 = entity.Purpose1,
            PurposeEnglish1 = entity.PurposeEnglish1,
            Purpose2 = entity.Purpose2,
            PurposeEnglish2 = entity.PurposeEnglish2
        };
    }
}