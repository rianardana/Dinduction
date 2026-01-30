namespace Dinduction.Web.Models;
public class ViewRecordMasterVM
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public bool IsTrue { get; set; }
        public DateTime RecordDate { get; set; }
        public int QuizNumber { get; set; }
        public int TrainerId { get; set; }
        public int Score { get; set; }
        public int TotalTrainingCount { get; set; }
        public int CompletedTrainingCount { get; set; }

        public string TrainingSummary => $"{CompletedTrainingCount} / {TotalTrainingCount}";
        public string TrainingProgress { get; set; }
        public int Failed { get; set; }
        

    }