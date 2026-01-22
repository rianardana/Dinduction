using System.ComponentModel.DataAnnotations;

namespace Dinduction.Web.Models;
public class ViewRecordTrainingVM
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
    }