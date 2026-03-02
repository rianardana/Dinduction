// Dinduction.Web/Models/LearningMaterialAdminVM.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;

// ✅ Untuk List/Edit di Admin Panel
public class LearningMaterialVM
{
    public int Id { get; set; }

    [Display(Name = "Training")]
    [Required(ErrorMessage = "Training is required")]
    public int TrainingId { get; set; }

    [Display(Name = "Training Name")]
    public string? TrainingName { get; set; } // ✅ Computed from join

    [Display(Name = "Material Title")]
    [Required(ErrorMessage = "Title is required")]
    public string? Title { get; set; } // ✅ Bisa dari nama file atau input manual

    [Display(Name = "File Path")]
    public string? FilePath { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }

    // ✅ Untuk admin dropdown
    public SelectList? ListTraining { get; set; }
}

// ✅ Untuk Upload/Create materi baru
public class LearningMaterialUploadVM
{
    [Display(Name = "Training")]
    [Required(ErrorMessage = "Training is required")]
    public int TrainingId { get; set; }

    [Display(Name = "Material Title")]
    [Required(ErrorMessage = "Title is required")]
    public string? Title { get; set; }

    [Display(Name = "PDF/Video File")]
    [Required(ErrorMessage = "Please select a file")]
    public IFormFile? File { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
    
    public SelectList? ListTraining { get; set; }
}