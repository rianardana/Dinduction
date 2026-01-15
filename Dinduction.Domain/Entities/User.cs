using System;
using System.Collections.Generic;

namespace Dinduction.Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public int? RoleId { get; set; }

    public string? EmployeeName { get; set; }

    public DateOnly? StartTraining { get; set; }

    public DateOnly? EndTraining { get; set; }

    public string? Department { get; set; }

    public virtual ICollection<ParticipantUser> ParticipantUsers { get; set; } = new List<ParticipantUser>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
}
