using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class VMasterQuestion
{
    public int Id { get; set; }

    public int? ParticipantId { get; set; }

    public int? TrainingId { get; set; }

    public string? TrainingName { get; set; }

    public string? Purpose1 { get; set; }

    public string? PurposeEnglish1 { get; set; }

    public string? EvaluationForm { get; set; }

    public string? Purpose2 { get; set; }

    public string? PurposeEnglish2 { get; set; }

    public DateOnly? FormDateRegistration { get; set; }

    public string? FormNumberRegistration { get; set; }

    public DateTime? RecordDate { get; set; }
}
