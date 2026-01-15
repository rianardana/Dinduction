using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class VRecordTraining
{
    public int? ParticipantId { get; set; }

    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? EmployeeName { get; set; }

    public int? TrainingId { get; set; }

    public string? TrainingName { get; set; }
}
