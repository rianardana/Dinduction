using System.ComponentModel.DataAnnotations;

namespace Dinduction.Web.Models;

    public class MasterTrainingVM
    {
        public int Id { get; set; }
        [Display(Name = "Role Name")]
        public int? SectionId { get; set; }
        public string TrainingName { get; set; }
        public string EvaluationForm { get; set; }
        public string Purpose1 { get; set; }
        public string PurposeEnglish1 { get; set; }
        public string Purpose2 { get; set; }
        public string PurposeEnglish2 { get; set; }
        public DateOnly? FormDateRegistration { get; set; }
        public string FormNumberRegistration { get; set; }
        public bool? IsActive { get; set; }

    
    }
