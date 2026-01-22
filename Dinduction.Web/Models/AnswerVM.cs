using System.ComponentModel.DataAnnotations;

namespace Dinduction.Web.Models;

public class AnswerVM
    {
        public AnswerVM()
        {

        }
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string RightAnswer { get; set; }
    }