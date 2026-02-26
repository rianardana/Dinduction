
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;

public class LearningMaterialVM
{
    public int Id { get; set; }

    [Display(Name = "Training")]
    [Required(ErrorMessage = "Training is required")]
    public int TrainingId { get; set; }

    [Display(Name = "Training Name")]
    public string TrainingName { get; set; }

    [Display(Name = "Material Title")]
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; }

    [Display(Name = "File Path")]
    public string FilePath { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }

    // ✅ Untuk tracking progress user
    [Display(Name = "Completed")]
    public bool IsCompleted { get; set; }

    // ✅ Dropdown helper
    public SelectList ListTraining { get; set; }
}

public class LearningMaterialDetailVM
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FilePath { get; set; }
    public string TrainingName { get; set; }
    public bool IsCompleted { get; set; }
    public int TrainingYear { get; set; } = DateTime.Now.Year;
}


public class LearningMaterialUploadVM
{
    [Display(Name = "Training")]
    [Required(ErrorMessage = "Training is required")]
    public int TrainingId { get; set; }

    [Display(Name = "Material Title")]
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; }

    [Display(Name = "PDF File")]
    [Required(ErrorMessage = "Please select a PDF file")]
    public IFormFile FilePdf { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
    
    public SelectList ListTraining { get; set; }
}