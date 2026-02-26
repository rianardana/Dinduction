using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;
public class UserVM
{
    public int Id { get; set; }
    
    [Display(Name = "User Name")]
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; }
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    public string Department { get; set; }

    // Password change properties - only for edit
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    public string ConfNewPassword { get; set; }
    
    [Display(Name = "Employee Name")]
    [Required(ErrorMessage = "Employee Name is required")]
    public string EmployeeName { get; set; }
    
    public SelectList ListRole { get; set; } 
    
    [Display(Name = "Role")]
    [Required(ErrorMessage = "Role is required")]
    public string RoleId { get; set; }
    
    [Display(Name = "Role")]
    public string RoleName { get; set; }
    
    
    [Display(Name = "Start Training")]
    [DataType(DataType.Date)]
    public DateOnly? StartTraining { get; set; }
    
    [Display(Name = "End Training")]
    [DataType(DataType.Date)]
    public DateOnly? EndTraining { get; set; } 
    public string TrainingType { get; set; } = "I";
    public SelectList ListTrainingType { get; set; }
}