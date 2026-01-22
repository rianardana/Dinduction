using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;

public class RecordTrainingVM
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string ParticipantName { get; set; }
        public int QuestionId { get; set; }
        public string QuestionName { get; set; }
        public string UserAnswer { get; set; }
        public bool IsTrue { get; set; }
        public DateTime RecordDate { get; set; }
        public string SelectedOption { get; set; }
        public int NumberQuestion { get; set; }
        public int QuizNumber { get; set; }
        public int Score { get; set; }
        public int TrainerId { get; set; }
    }