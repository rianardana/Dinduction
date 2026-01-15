using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class VRecordResult
{
    public int Id { get; set; }

    public int? ParticipantId { get; set; }

    public int? QuestionId { get; set; }

    public string? UserAnswer { get; set; }

    public bool? IsTrue { get; set; }

    public int? TrainerId { get; set; }

    public DateOnly? TrainingDate { get; set; }

    public string? UserName { get; set; }

    public string? EmployeeName { get; set; }

    public string? TrainingName { get; set; }

    public string? EvaluationForm { get; set; }

    public string? Purpose1 { get; set; }

    public string? PurposeEnglish1 { get; set; }

    public string? Purpose2 { get; set; }

    public string? PurposeEnglish2 { get; set; }

    public DateOnly? FormDateRegistration { get; set; }

    public string? FormNumberRegistration { get; set; }

    public string? Signature { get; set; }

    public string? QuestionTraining { get; set; }

    public int? Number { get; set; }

    public int? TrainerName { get; set; }

    public int? TrainingId { get; set; }

    public int? QuizNumber { get; set; }

    public int Score { get; set; }

    public string? RightAnswer { get; set; }

    public string? OptionA { get; set; }

    public string? OptionB { get; set; }

    public string? OptionC { get; set; }
}
