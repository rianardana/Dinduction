public class RefreshComparisonDto
{
    public int ParticipantId { get; set; }
    public int TrainingId { get; set; }
    public string EmployeeName { get; set; }
    public string TrainingName { get; set; }
    public int? PreTestScore { get; set; }
    public int? PostTestScore { get; set; }
    public DateTime RecordDate { get; set; }
    public int TotalTraining { get; set; }
    public int PreTestDone     { get; set; }
    public int PostTestPassed   { get; set; }
}