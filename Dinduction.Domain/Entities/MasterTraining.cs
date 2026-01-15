using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class MasterTraining
{
    public int Id { get; set; }

    public int? SectionId { get; set; }

    public string? TrainingName { get; set; }

    public string? EvaluationForm { get; set; }

    public string? Purpose1 { get; set; }

    public string? PurposeEnglish1 { get; set; }

    public string? Purpose2 { get; set; }

    public string? PurposeEnglish2 { get; set; }

    public DateOnly? FormDateRegistration { get; set; }

    public string? FormNumberRegistration { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ParticipantUser> ParticipantUsers { get; set; } = new List<ParticipantUser>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<RecordTraining> RecordTrainings { get; set; } = new List<RecordTraining>();

    public virtual Section? Section { get; set; }
}
