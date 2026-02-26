using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class LearningMaterial
{
    public int Id { get; set; }

    public int? TrainingId { get; set; }

    public string? FilePath { get; set; }

    public bool? IsActive { get; set; }

    public virtual MasterTraining? Training { get; set; }

    public virtual ICollection<UserLearningProgress> UserLearningProgresses { get; set; } = new List<UserLearningProgress>();
}
