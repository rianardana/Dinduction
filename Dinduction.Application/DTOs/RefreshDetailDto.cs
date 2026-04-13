public class RefreshDetailDto
{
    public int ParticipantId { get; set; }
    public int TrainingId { get; set; }
    public string TrainingName { get; set; }
    public int? PreTestScore { get; set; }
    public int? PostTestScore { get; set; }
    public int QuizNumber { get; set; }
}