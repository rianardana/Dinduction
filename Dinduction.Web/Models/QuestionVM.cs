using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Dinduction.Web.Models;
public class QuestionVM
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string SectionName { get; set; }
        public string EvaluationForm { get; set; }
        public string QuestionTraining { get; set; }
        public int Number { get; set; }
        public string ImageQuestion { get; set; }
        public List<AnswerVM> Answers { get; set; } = new List<AnswerVM>();
    }