using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class ParticipantUser
{
    public int Id { get; set; }

    public int? TrainerId { get; set; }

    public int? UserId { get; set; }

    public int? TrainingId { get; set; }

    public DateTime? TrainingDate { get; set; }

    public int? SectionTrainerId { get; set; }

    public virtual ICollection<RecordTraining> RecordTrainings { get; set; } = new List<RecordTraining>();

    public virtual Trainer? Trainer { get; set; }

    public virtual MasterTraining? Training { get; set; }

    public virtual User? User { get; set; }
}
