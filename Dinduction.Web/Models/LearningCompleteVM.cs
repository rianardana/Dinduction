// File: Dinduction.Web/Models/LearningCompleteVM.cs
namespace Dinduction.Web.Models;

public class LearningCompleteVM
{
    public string? EmployeeName { get; set; }
    public string? TrainingName { get; set; }
    public DateTime CompletedDate { get; set; }
    public string? NextStepUrl { get; set; }
}