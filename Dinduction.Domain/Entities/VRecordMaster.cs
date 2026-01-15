using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class VRecordMaster
{
    public int Id { get; set; }

    public int? ParticipantId { get; set; }

    public string? UserName { get; set; }

    public string? EmployeeName { get; set; }

    public int? TrainingId { get; set; }

    public string? TrainingName { get; set; }

    public bool? IsTrue { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? QuizNumber { get; set; }

    public int Score { get; set; }

    public int? TrainerId { get; set; }
}
