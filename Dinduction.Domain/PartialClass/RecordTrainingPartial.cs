
using System.ComponentModel.DataAnnotations.Schema;

namespace Dinduction.Domain.Entities;

public partial class RecordTraining
{

    [NotMapped] 
    public int Score { get; set; }
}