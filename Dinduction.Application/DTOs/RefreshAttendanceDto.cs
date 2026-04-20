
public class RefreshAttendanceDto
{
    public string UserName { get; set; }
    public string EmployeeName { get; set; }
    public string Department { get; set; }
    public DateTime TrainingDate { get; set; }
    public string TrainingType { get; set; }
    public bool HasPreTest { get; set; }
    public bool HasPostTest { get; set; }
    // Hadir = PreTest DAN PostTest ada
    public bool IsPresent => HasPreTest && HasPostTest;
}