using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class RecordTraining
{
    public int Id { get; set; }

    public int? ParticipantId { get; set; }

    public int? QuestionId { get; set; }

    public string? UserAnswer { get; set; }

    public bool? IsTrue { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? NumberQuestion { get; set; }

    public int? TrainingId { get; set; }

    public int? QuizNumber { get; set; }

    public int? TrainerId { get; set; }
   

    public virtual ParticipantUser? Participant { get; set; }

    public virtual Question? Question { get; set; }

    public virtual Trainer? Trainer { get; set; }

    public virtual MasterTraining? Training { get; set; }
}
