using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class VResult
{
    public int Id { get; set; }

    public int? ParticipantId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? TrainingId { get; set; }

    public bool? IsTrue { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? EmployeeName { get; set; }

    public string? TrainingName { get; set; }

    public int Score { get; set; }
}
