using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class Trainer
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? SectionId { get; set; }

    public string? Signature { get; set; }

    public virtual ICollection<ParticipantUser> ParticipantUsers { get; set; } = new List<ParticipantUser>();

    public virtual ICollection<RecordTraining> RecordTrainings { get; set; } = new List<RecordTraining>();

    public virtual Section? Section { get; set; }

    public virtual User? User { get; set; }
}
