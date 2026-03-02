// File: Dinduction.Web/Models/LearningStudyVM.cs
namespace Dinduction.Web.Models;

public class LearningStudyVM
{
    public int TrainingId { get; set; }
    public string? TrainingName { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }
    
    // ✅ List materi - PAKAI UserLearningMaterialVM
    public List<UserLearningMaterialVM>? Materials { get; set; } = new();
    
    // ✅ Progress summary
    public int TotalMaterials { get; set; }
    public int CompletedMaterials { get; set; }
    
    // ✅ Computed properties (read-only)
    public int ProgressPercent => TotalMaterials > 0 ? (CompletedMaterials * 100 / TotalMaterials) : 0;
    public bool IsAllCompleted => ProgressPercent == 100;
}