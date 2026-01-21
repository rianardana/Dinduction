using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Dinduction.Web.Models;
public class ParticipantUserVM
{
    public int Id { get; set; }
    public string? TrainingName { get; set; }
    public string? TrainerName { get; set; }
    public int SectionTrainerId { get; set; }
    public List<SelectListItem> ListParticipant { get; set; } = new();
    public List<int> SelectedParticipants { get; set; } = new();
    [Display(Name = "User Name")]
    public string? UserName { get; set; }
    [Display(Name = "Employee Name")]
    public string? EmployeeName { get; set; }
    public string? Department { get; set; }

}