using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class Question
{
    public int Id { get; set; }

    public int? TrainingId { get; set; }

    public string? QuestionTraining { get; set; }

    public int? Number { get; set; }

    public string? ImageQuestion { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<RecordTraining> RecordTrainings { get; set; } = new List<RecordTraining>();

    public virtual MasterTraining? Training { get; set; }
}
