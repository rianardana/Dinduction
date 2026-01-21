// Dinduction.Application/DTOs/TrainingDateDTO.cs
namespace Dinduction.Application.DTOs;

public class TrainingDateDTO
{
    public DateTime Date { get; set; }
    public List<TrainingDto> Trainings { get; set; } = new();
}