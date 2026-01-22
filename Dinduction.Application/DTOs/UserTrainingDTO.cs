// Dinduction.Application/DTOs/UserTrainingDTO.cs
namespace Dinduction.Application.DTOs;

public class UserTrainingDTO
{
    public int TrainingId { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public bool? IsPass { get; set; }

    public string Status => !IsPass.HasValue 
        ? "NOT TAKEN" 
        : IsPass.Value ? "PASS" : "FAILED";

    public string StatusClass => !IsPass.HasValue 
        ? "not-taken" 
        : IsPass.Value ? "pass" : "failed";
}