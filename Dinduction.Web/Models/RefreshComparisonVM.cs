namespace Dinduction.Web.Models;
public class RefreshComparisonVM
{
    public int ParticipantId { get; set; }
    public string EmployeeName { get; set; }
    public DateTime RecordDate { get; set; }
    public int TotalTraining { get; set; }
    public int PostTestPassed  { get; set; }
    public int    PreTestDone    { get; set; }
    public string PostTestSummary => $"{PostTestPassed } / {TotalTraining}";
}


public class RefreshDetailVM
{
    public int ParticipantId { get; set; }
    public int TrainingId { get; set; }
    public string TrainingName { get; set; }
    public int? PreTestScore { get; set; }
    public int? PostTestScore { get; set; }
    public int QuizNumber { get; set; }
    public int? Improvement => (PostTestScore.HasValue && PreTestScore.HasValue)
        ? PostTestScore - PreTestScore : null;
    public bool PostTestFailed => PostTestScore == null || PostTestScore < 80;
}