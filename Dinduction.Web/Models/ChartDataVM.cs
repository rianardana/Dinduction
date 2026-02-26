namespace Dinduction.Web.Models;
public class ChartDataVM
{
    public string[] Labels { get; set; } = [];
    public string[] DatasetLabels { get; set; } = [];
    public string[] Colors { get; set; } = [];
    public int[][] DatasetDatas { get; set; } = [];
}
  public class GraphicVM
{
    public ChartDataVM BarChart { get; set; } = new ChartDataVM();
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    

}