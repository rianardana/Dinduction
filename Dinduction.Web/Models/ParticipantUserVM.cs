using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;

public class ParticipantUserVM
{
    public ParticipantUserVM()
    {
        ListParticipant = new List<SelectListItem>();
        SelectedParticipants = new List<int>();
        Participants = new List<ParticipantUserVM>();
    }

    public int Id { get; set; }
    
    [Display(Name = "Trainer Name")]
    public int TrainerId { get; set; }
    
    [Display(Name = "Training Name")]
    public int TrainingId { get; set; }
    
    [Display(Name = "Trainer Name")]
    public string? TrainerName { get; set; }
    
    [Display(Name = "Training Name")]
    public string? TrainingName { get; set; }

    public string? Department { get; set; }
    
    [Display(Name = "User Name")]
    public int UserId { get; set; }
    
    [Display(Name = "User Name")]
    public string? UserName { get; set; }
    
    [Display(Name = "Employee Name")]
    public string? EmployeeName { get; set; }
    
    [Display(Name = "Training Date")]
    public DateTime TrainingDate { get; set; }
    
    public int SectionTrainerId { get; set; }

    public List<SelectListItem> ListParticipant { get; set; }
    public SelectList? ListTraining { get; set; }
    public List<int> SelectedParticipants { get; set; }
    public List<ParticipantUserVM> Participants { get; set; }
}