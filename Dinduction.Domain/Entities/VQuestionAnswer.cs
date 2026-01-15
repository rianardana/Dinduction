using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class VQuestionAnswer
{
    public int Id { get; set; }

    public int? TrainingId { get; set; }

    public string? QuestionTraining { get; set; }

    public int? Number { get; set; }

    public string? RightAnswer { get; set; }

    public string? OptionA { get; set; }

    public string? OptionB { get; set; }

    public string? OptionC { get; set; }

    public string? ImageQuestion { get; set; }

    public string? ImageRight { get; set; }

    public string? ImageA { get; set; }

    public string? ImageB { get; set; }

    public string? ImageC { get; set; }
}
