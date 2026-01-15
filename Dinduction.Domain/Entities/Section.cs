using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class Section
{
    public int Id { get; set; }

    public string? SectionName { get; set; }

    public virtual ICollection<MasterTraining> MasterTrainings { get; set; } = new List<MasterTraining>();

    public virtual ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
}
