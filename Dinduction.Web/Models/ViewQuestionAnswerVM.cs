using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;
    public class ViewQuestionAnswerVM
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int QuizNumber { get; set; }
        public string QuestionTraining { get; set; }
        public string RightAnswer { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }

        public string SelectedOption { get; set; }

        public string ImageQuestion { get; set; }
        public int Number { get; set; }

        public string ImageRight { get; set; }

        public string ImageA { get; set; }
        public string ImageB { get; set; }
        public string ImageC { get; set; }
        public int TrainerId { get; set; }

        public List<AnswerOptionVM> ShuffledOptions { get; set; } = new List<AnswerOptionVM>();
    }

        public class AnswerOptionVM
        {
            public string Key { get; set; }        
            public string Text { get; set; }   
            public string ImagePath { get; set; }   
        }