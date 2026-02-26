using System;
using System.Collections.Generic;
namespace Dinduction.Domain.Entities;

public partial class UserLearningProgress
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? MaterialId { get; set; }

    public int? TrainingYear { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CompletedDate { get; set; }

    public virtual LearningMaterial? Material { get; set; }

    public virtual User? User { get; set; }
}
