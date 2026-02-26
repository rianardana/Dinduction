using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dinduction.Web.Models;
using Dinduction.Application.Interfaces;

namespace Dinduction.Web.Controllers;
public class DashboardController : Controller
{
    private readonly IRecordTrainingService _recordTrainingService;

    public DashboardController(IRecordTrainingService recordTrainingService)
    {
        _recordTrainingService = recordTrainingService;
    }

    public IActionResult CountParticipant()
    {
        return View();
    }

    [HttpGet]
    public async Task<JsonResult> PopulateParticipant(string year = null) // ← Tambahkan async
    {
        try
        {
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            var chartData = new ChartDataVM();
            var startDate = new DateTime(Convert.ToInt32(year), 1, 1);
            var endDate = new DateTime(Convert.ToInt32(year), 12, 31);

            // ✅ TAMBAHKAN AWAIT DI SINI!
            var records = await _recordTrainingService.GetAllChartAsync(startDate, endDate);

            // Generate all months
            var months = Enumerable.Range(1, 12)
                .Select(m => new DateTime(Convert.ToInt32(year), m, 1))
                .ToList();

            // Calculate unique participants per month
            var participantCounts = months.Select(month =>
            {
                var monthEnd = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
                return records
                    .Where(r => r.RecordDate >= month && r.RecordDate <= monthEnd)
                    .Select(r => r.ParticipantId)
                    .Distinct()
                    .Count();
            }).ToArray();

            chartData.Labels = months.Select(m => m.ToString("MMM-yyyy")).ToArray();
            chartData.DatasetLabels = new[] { "Participants" };
            chartData.Colors = new[] { "#28a745" };
            chartData.DatasetDatas = new[] { participantCounts };

            return Json(new { data = chartData });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
}