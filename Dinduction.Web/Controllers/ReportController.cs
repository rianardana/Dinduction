using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Dinduction.Application.Models;
using Dinduction.Web.Models;
using Dinduction.Application.Interfaces;
using ClosedXML.Excel;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace Dinduction.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IRecordTrainingService _recordTrainingService;
        private readonly IParticipantService _participantService;
        private readonly ITrainingService _masterTrainingService;
        private readonly ITrainerService _trainerService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ReportController(
            IRecordTrainingService recordTrainingService,
            IParticipantService participantService,
            ITrainingService masterTrainingService,
            ITrainerService trainerService,
            IUserService userService,
            IMapper mapper)
        {
            _recordTrainingService = recordTrainingService;
            _participantService = participantService;
            _masterTrainingService = masterTrainingService;
            _trainerService = trainerService;
            _userService = userService;
            _mapper = mapper;
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        [HttpPost]
        public async Task<JsonResult> CustomServerSide(DataTableAjaxPostModel model)
        {
            try
            {
                var (data, totalCount) = await _recordTrainingService.SearchRecordAsync(model);
                var mappedData = _mapper.Map<List<RecordTrainingVM>>(data);

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = mappedData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        public async Task<IActionResult> GetPresence(DateTime? date, int? trainingId)
        {
            var model = new ParticipantUserVM();

            try
            {
                var selectedDate = date ?? DateTime.Today;
                var groupedTrainings = await _participantService.GetTrainingGroupedByDateAsync();
                ViewBag.GroupedTrainings = groupedTrainings;

                if (trainingId == null || trainingId == 0)
                {
                    model.Participants = new List<ParticipantUserVM>();
                    return View(model);
                }

                var data = await _participantService.GetPresenceAsync(selectedDate, trainingId.Value);
                model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        public async Task<IActionResult> GetPresenceByTrainer(DateTime? date, int? trainingId)
        {
            var model = new ParticipantUserVM();

            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                var selectedDate = date ?? DateTime.Today;

                var groupedTrainings = await _participantService.GetTrainingGroupedByDateByTrainerAsync(trainerId);
                ViewBag.GroupedTrainings = groupedTrainings;

                if (trainingId == null || trainingId == 0)
                {
                    model.Participants = new List<ParticipantUserVM>();
                    return View(model);
                }

                var data = await _participantService.GetPresenceByTrainerAsync(selectedDate, trainingId.Value, trainerId);
                model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetInductionTrainingDates()
        {
            try
            {
                // Ambil tanggal unik dari RecordTraining yang tipenya 'I' (Induction)
                var dates = await _participantService.GetTrainingDatesByTypeAsync("I");

                var formatted = dates
                    .Select(d => d.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .ToList();

                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return Ok(new List<string>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAdminRefreshTrainingDates()
        {
            try
            {
                // Ambil tanggal unik dari RecordTraining yang tipenya 'R' (Refresh)
                var dates = await _participantService.GetRefreshTrainingDatesAsync();

                var formatted = dates
                    .Select(d => d.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .ToList();

                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return Ok(new List<string>());
            }
        }

        public async Task<IActionResult> PreviewTrainingForm(DateTime? date, int? trainingId)
        {
            var model = new ParticipantUserVM();
            
            ViewBag.TrainingId = trainingId; 

            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                var selectedDate = date ?? DateTime.Today;

                var data = await _participantService.GetPresenceByTrainerAsync(selectedDate, trainingId ?? 0, trainerId);
                var training = await _masterTrainingService.GetByIdAsync(trainingId ?? 0);

                model.TrainingName = training?.TrainingName ?? "N/A";
                model.TrainingDate = selectedDate.Date;
                model.TrainerName = await _userService.GetUserNameByIdAsync(userId);
                
                decimal duration = training?.InductionDuration ?? 0;
                ViewBag.TrainingType = "Internal";
                ViewBag.Duration = duration > 0 ? $"{duration} Minutes" : "N/A";
                
                model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Duration = "N/A";
            }
            
            return View("_TrainingFormPdf", model);
        }

        public async Task<IActionResult> PreviewTrainingFormAdmin(DateTime? date, int? trainingId)
{
    if (!date.HasValue || !trainingId.HasValue || trainingId == 0) return BadRequest();
    
    ViewBag.TrainingId = trainingId;

    try
    {
        var training = await _masterTrainingService.GetByIdAsync(trainingId.Value);
        if (training == null) return NotFound();

        var trainerId = await _participantService.GetTrainerByDateAndTrainingAsync(date.Value, trainingId.Value);
        
        string trainerName = "N/A";
        if (trainerId > 0)
        {
            var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
            if (userTrainerId > 0)
            {
                trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);
            }
        }

        var participants = await _participantService.GetPresenceAsync(date.Value.Date, trainingId.Value);
        
        decimal duration = training.InductionDuration ?? 0;

        ViewBag.Duration = duration > 0 ? $"{duration} Minutes" : "N/A";
        ViewBag.TrainingType = "Internal";

        var mappedParticipants = _mapper.Map<List<ParticipantUserVM>>(participants);
        
        var model = new ParticipantUserVM
        {
            TrainingName = training.TrainingName,
            TrainingDate = date.Value.Date,
            TrainerName = trainerName,
            Participants = mappedParticipants
        };

        return View("_TrainingFormPdf", model);
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", ex.Message);
        ViewBag.Duration = "N/A";
        return View("_TrainingFormPdf", new ParticipantUserVM { TrainingName = "Error", Participants = new List<ParticipantUserVM>() });
    }
}

       public async Task<IActionResult> PreviewRefreshFormAdmin(DateTime? date, int? trainingId)
{
    if (!date.HasValue || !trainingId.HasValue || trainingId == 0) return BadRequest();
    
    ViewBag.TrainingId = trainingId;

    try
    {
        var training = await _masterTrainingService.GetByIdAsync(trainingId.Value);
        if (training == null) return NotFound();

        var trainerId = await _participantService.GetTrainerByDateAndTrainingAsync(date.Value, trainingId.Value);
        
        string trainerName = "N/A";
        if (trainerId > 0)
        {
            var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
            if (userTrainerId > 0)
            {
                trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);
            }
        }

        var participants = await _participantService.GetPresenceAsync(date.Value.Date, trainingId.Value);
        
        // ✅ FIX: Jika RefreshDuration kosong, pakai InductionDuration sebagai fallback sementara
        decimal duration = training.RefreshDuration ?? training.InductionDuration ?? 0;

        ViewBag.Duration = duration > 0 ? $"{duration} Minutes" : "N/A";
        ViewBag.TrainingType = "Internal";

        var mappedParticipants = _mapper.Map<List<ParticipantUserVM>>(participants);
        
        var model = new ParticipantUserVM
        {
            TrainingName = training.TrainingName,
            TrainingDate = date.Value.Date,
            TrainerName = trainerName,
            Participants = mappedParticipants
        };

        return View("_TrainingFormPdf", model);
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", ex.Message);
        ViewBag.Duration = "N/A";
        return View("_TrainingFormPdf", new ParticipantUserVM { TrainingName = "Error", Participants = new List<ParticipantUserVM>() });
    }
}

        [HttpGet]
    public async Task<IActionResult> DownloadTrainingFormAsPdf(DateTime? date, int? trainingId)
    {
        Console.WriteLine($"[DOWNLOAD DEBUG] Date: {date}, TrainingId: {trainingId}");
        
        var model = new ParticipantUserVM();
        try
        {
            var userId = GetCurrentUserId();
            var trainerId = await _trainerService.GetTrainerIdAsync(userId);
            var selectedDate = date ?? DateTime.Today;

            Console.WriteLine($"[DOWNLOAD DEBUG] UserId: {userId}, TrainerId: {trainerId}");

            var data = await _participantService.GetPresenceByTrainerAsync(selectedDate, trainingId ?? 0, trainerId);
            
            Console.WriteLine($"[DOWNLOAD DEBUG] Data Count from Service: {data?.Count ?? 0}");
            
            var training = await _masterTrainingService.GetByIdAsync(trainingId ?? 0);

            model.TrainingName = training?.TrainingName ?? "N/A";
            model.TrainingDate = selectedDate.Date;
            model.TrainerName = await _userService.GetUserNameByIdAsync(userId);
            
            decimal duration = training?.InductionDuration ?? 0;
            model.DurationDisplay = duration > 0 ? $"{duration} Minutes" : "N/A";
            model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);
            
            Console.WriteLine($"[DOWNLOAD DEBUG] Mapped Participants Count: {model.Participants?.Count ?? 0}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DOWNLOAD ERROR] {ex.Message}");
            model.DurationDisplay = "N/A";
            model.Participants = new List<ParticipantUserVM>();
        }

        if (model.Participants == null || !model.Participants.Any())
        {
            Console.WriteLine("[DOWNLOAD WARNING] No participants found! Returning empty PDF.");
        }

        var pdfBytes = GenerateTrainingFormPdf(model);
        return File(pdfBytes, "application/pdf", $"Training_Form_{model.TrainingName}_{(date ?? DateTime.Today):yyyyMMdd}.pdf");
    }

        [HttpGet]
        public async Task<IActionResult> DownloadTrainingFormAdminAsPdf(DateTime? date, int? trainingId)
        {
            if (!date.HasValue || !trainingId.HasValue || trainingId == 0) return BadRequest();
            try
            {
                var training = await _masterTrainingService.GetByIdAsync(trainingId.Value);
                if (training == null) return NotFound();

                var trainerId = await _participantService.GetTrainerByDateAndTrainingAsync(date.Value, trainingId.Value);
                
                string trainerName = "N/A";
                if (trainerId > 0)
                {
                    var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
                    if (userTrainerId > 0)
                    {
                        trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);
                    }
                }

                var participants = await _participantService.GetPresenceAsync(date.Value.Date, trainingId.Value);
                
                decimal duration = training.InductionDuration ?? 0;
                
                var mappedParticipants = _mapper.Map<List<ParticipantUserVM>>(participants);
                
                var model = new ParticipantUserVM
                {
                    TrainingName = training.TrainingName,
                    TrainingDate = date.Value.Date,
                    TrainerName = trainerName,
                    Participants = mappedParticipants,
                    DurationDisplay = duration > 0 ? $"{duration} Minutes" : "N/A"
                };

                var pdfBytes = GenerateTrainingFormPdf(model);
                return File(pdfBytes, "application/pdf", $"Training_Form_Admin_{model.TrainingName}_{date.Value:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to generate PDF");
            }
        }


        public async Task<IActionResult> DownloadTrainingForm(DateTime? date, int? trainingId)
        {
            var model = new ParticipantUserVM();

            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                var selectedDate = date ?? DateTime.Today;

                var data = await _participantService.GetPresenceByTrainerAsync(selectedDate, trainingId ?? 0, trainerId);
                var training = await _masterTrainingService.GetByIdAsync(trainingId ?? 0);

                model.TrainingName = training?.TrainingName ?? "N/A";
                model.TrainingDate = selectedDate.Date;
                model.TrainerName = await _userService.GetUserNameByIdAsync(userId);
                ViewBag.TrainingType = "Internal";

                model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);

                return View("_TrainingFormPdf", model);
            }
            catch (Exception ex)
            {
                model.TrainingName = "Gagal Generate PDF";
                model.Participants = new List<ParticipantUserVM>();
                return View("_TrainingFormPdf", model);
            }
        }

        public async Task<IActionResult> DownloadTrainingFormAdmin(DateTime? date, int? trainingId)
        {
            var model = new ParticipantUserVM();

            try
            {
                var selectedDate = date ?? DateTime.Today;
                var training = await _masterTrainingService.GetByIdAsync(trainingId ?? 0);

                model.TrainingName = training?.TrainingName ?? "N/A";
                model.TrainingDate = selectedDate.Date;

                var trainerId = await _participantService.GetTrainerByDateAndTrainingAsync(date.Value, trainingId.Value);
                var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
                model.TrainerName = await _userService.GetUserNameByIdAsync(userTrainerId);

                var data = await _participantService.GetPresenceAsync(selectedDate.Date, trainingId ?? 0);
                model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);

                return View("_TrainingFormPdf", model);
            }
            catch (Exception ex)
            {
                model.TrainingName = "Gagal Generate PDF";
                model.Participants = new List<ParticipantUserVM>();
                return View("_TrainingFormPdf", model);
            }
        }

       public async Task<JsonResult> GetPresenceData(string date, int? trainingId)
{
    if (!DateTime.TryParse(date, out var selectedDate))
        return Json(new List<object>());

    try
    {
        var data = await _participantService.GetPresenceAsync(selectedDate, trainingId ?? 0);
        
        // Ambil list UserId
        var userIds = data
            .Select(p => p.UserId)
            .Where(id => id.HasValue && id.Value > 0)
            .Select(id => id.Value)
            .Distinct()
            .ToList();

        
        var recordMapping = await _participantService.GetOriginalParticipantIdsAsync(selectedDate, userIds);

        var mappedData = _mapper.Map<List<ParticipantUserVM>>(data);

        foreach (var vm in mappedData)
        {

            if (vm.UserId > 0 && recordMapping.TryGetValue(vm.UserId, out var realPid))
            {
                vm.Id = realPid;
            }
        }

        return Json(mappedData);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] GetPresenceData: {ex.Message}");
        return Json(new { error = ex.Message });
    }
}

        public async Task<IActionResult> GetPresenceDataByTrainer(string date, int? trainingId)
        {
            if (trainingId == null || trainingId == 0)
                return Ok(new List<object>());

            if (!DateTime.TryParse(date, out var selectedDate))
                return Ok(new List<object>());

            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Ok(new List<object>());

                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                
                var (participants, trainingTypes) = await _participantService
                    .GetPresenceByTrainerWithTrainingTypeAsync(selectedDate, trainingId.Value, trainerId);
                
                var mappedData = _mapper.Map<List<ParticipantUserVM>>(participants);
                
                foreach (var vm in mappedData)
                {
                    if (trainingTypes.TryGetValue(vm.Id, out var type))
                    {
                        vm.TrainingType = "Refreshment"; 
                    }
                }

                return Ok(mappedData);
            }
            catch (Exception ex)
            {
                return Ok(new List<object>());
            }
        }

        [HttpGet]
public async Task<JsonResult> GetInductionPresenceData(string date)
{
    if (!DateTime.TryParse(date, out var selectedDate))
        return Json(new List<object>());

    try
    {
        var data = await _participantService.GetInductionPresenceAsync(selectedDate);
        
        // FIX: Konversi int? ke int dengan benar
        var userIds = data
            .Select(p => p.UserId)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToList();

        var recordMapping = await _participantService.GetInductionOriginalParticipantIdsAsync(selectedDate, userIds);

        var mappedData = _mapper.Map<List<ParticipantUserVM>>(data);

        foreach (var vm in mappedData)
        {
            // vm.UserId adalah int (non-nullable), jadi langsung pakai
            if (vm.UserId > 0 && recordMapping.TryGetValue(vm.UserId, out var realPid))
            {
                vm.Id = realPid;
            }
        }

        return Json(mappedData);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] GetInductionPresenceData: {ex.Message}");
        return Json(new { error = ex.Message });
    }
}

        public async Task<JsonResult> GetTrainingDates()
        {
            try
            {
                var listDate = await _participantService.GetTrainingDatesAsync();
                var dates = listDate.Select(d => d.ToString("yyyy-MM-dd")).ToList();

                return Json(dates.ToArray());
            }
            catch (Exception ex)
            {
                return Json(new string[0]);
            }
        }

        public async Task<IActionResult> GetTrainingDatesByTrainer()
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                var listDate = await _participantService.GetTrainingDatesByTrainerAsync(trainerId);
                var dates = listDate.Select(d => d.ToString("yyyy-MM-dd")).ToList();

                return Ok(dates); 
            }
            catch (Exception ex)
            {
                return Ok(new List<string>());
            }
        }

        public async Task<IActionResult> MissingRefresh(DateTime? date)
        {
            var model = new ParticipantUserVM();
            
            try
            {
                var selectedDate = date ?? DateTime.Today;
                
                var data = await _participantService.GetMissingRefreshParticipantsByDateAsync(selectedDate);
                
                model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);
                model.TrainingDate = selectedDate;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        public async Task<JsonResult> GetTrainingsByDate(string date)
        {
            if (!DateTime.TryParse(date, out var targetDate))
                return Json(new List<object>());

            try
            {
                var trainings = await _participantService.GetScheduledTrainingsByDateAsync(targetDate);
                
                var result = trainings
                    .Select(t => new
                    {
                        value = t.Id,
                        text = t.TrainingName ?? "Unknown"
                    })
                    .ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetTrainingsByDate: {ex.Message}");
                return Json(new List<object>());
            }
        }

        public async Task<IActionResult> GetTrainingsByDateByTrainer(string date)
        {
            if (!DateTime.TryParse(date, out var targetDate))
                return Ok(new List<object>());

            try
            {
                var userId = GetCurrentUserId();
                
                if (userId == 0)
                {
                    return Ok(new List<object>());
                }

                var trainerId = await _trainerService.GetTrainerIdAsync(userId);

                var groupedTrainings = await _participantService.GetTrainingGroupedByDateByTrainerAsync(trainerId);

                var trainings = groupedTrainings
                    .Where(t => t.Date == targetDate.Date)
                    .SelectMany(t => t.Trainings)
                    .Select(tr => new
                    {
                        value = tr.TrainingId,
                        text = tr.TrainingName
                    })
                    .ToList();

                return Ok(trainings);
            }
            catch (Exception ex)
            {
                return Ok(new List<object>());
            }
        }

        public async Task<IActionResult> RefreshReport()
        {
            return View();
        }

        public async Task<IActionResult> RefreshReportAdmin()
        {
        
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetRefreshPresenceData(DateTime? date, int? trainingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);

                if (!date.HasValue || !trainingId.HasValue || trainingId == 0)
                    return Json(new List<object>());

                var data = await _participantService.GetRefreshPresenceByTrainerAsync(
                    date.Value, trainingId.Value, trainerId);

                var mapped = _mapper.Map<List<RefreshAttendanceVM>>(data);
                return Json(mapped);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRefreshTrainingsByDate(DateTime? date)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);

                var trainings = await _recordTrainingService
                    .GetTrainingsByDateByTrainerAsync(date, trainerId, "R"); 

                return Json(trainings);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRefreshTrainingDates()
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);

                var dates = await _participantService
                    .GetTrainingDatesByTrainerAndTypeAsync(trainerId, "R");

                var formatted = dates
                    .Select(d => d.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .ToList();

                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return Ok(new List<string>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportAttendanceToExcel(DateTime date, int trainingId)
        {
            try
            {
                var data = await _participantService.GetPresenceAsync(date, trainingId);
                var participants = _mapper.Map<List<ParticipantUserVM>>(data);

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Attendance");
                    
                    var headers = new[] { "No Badge", "Employee Name", "Training Date", "Department", "Training Type" };
                    for (int col = 0; col < headers.Length; col++)
                    {
                        worksheet.Cell(1, col + 1).Value = headers[col];
                    }
                    
                    var headerRange = worksheet.Range(1, 1, 1, headers.Length);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#3b82f6");
                    headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    
                    int row = 2;
                    foreach (var p in participants)
                    {
                        worksheet.Cell(row, 1).Value = p.UserName ?? "-";
                        worksheet.Cell(row, 2).Value = p.EmployeeName ?? "-";
                        worksheet.Cell(row, 3).Value = p.TrainingDate.ToString("dd-MMM-yyyy") ?? "-";
                        worksheet.Cell(row, 4).Value = p.Department ?? "-";
                        
                        var typeCode = p.TrainingType ?? "I"; 
                        worksheet.Cell(row, 5).Value = typeCode switch
                        {
                            "I" => "Induction",
                            "R" => "Refresh",
                            _ => typeCode
                        };
                        
                        row++;
                    }
                    
                    worksheet.Columns().AdjustToContents();
                    
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Attendance_{date:yyyyMMdd}_Training{trainingId}.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to export Excel: {ex.Message}");
                return BadRequest("Failed to generate Excel file");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportMissingRefreshToExcel(DateTime date)
        {
            try
            {
                var data = await _participantService.GetMissingRefreshParticipantsByDateAsync(date);
                
                var participants = _mapper.Map<List<ParticipantUserVM>>(data);

                var uniqueParticipants = participants
                    .GroupBy(p => p.UserName)
                    .Select(g => g.First())
                    .ToList();

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Missing Refresh");
                    
                    var headers = new[] { "No Badge", "Employee Name", "Department", "Status" };
                    for (int col = 0; col < headers.Length; col++)
                    {
                        worksheet.Cell(1, col + 1).Value = headers[col];
                    }
                    
                    var headerRange = worksheet.Range(1, 1, 1, headers.Length);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#dc2626");
                    headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    
                    int row = 2;
                    foreach (var p in uniqueParticipants)
                    {
                        worksheet.Cell(row, 1).Value = p.UserName ?? "-";
                        worksheet.Cell(row, 2).Value = p.EmployeeName ?? "-";
                        worksheet.Cell(row, 3).Value = p.Department ?? "-";
                        worksheet.Cell(row, 4).Value = "Belum Post-Test";
                        row++;
                    }
                    
                    worksheet.Columns().AdjustToContents();
                    
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Missing_Refresh_{date:yyyyMMdd}.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to export Excel: {ex.Message}");
                return BadRequest("Failed to generate Excel file");
            }
        }

        private byte[] GenerateTrainingFormPdf(ParticipantUserVM model)
{
    Console.WriteLine($"[PDF DEBUG] Participants Count: {model.Participants?.Count ?? 0}");

    return Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(15);
            page.PageColor(Colors.White);

            page.Header().AlignCenter().Text("TRAINING ATTENDANCE FORM").FontSize(16).Bold().Underline();

            page.Content().Column(col =>
            {
                col.Spacing(5);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Black).Padding(5).Column(leftCol =>
                    {
                        leftCol.Item().Text("A) Type of training:").Bold();
                        leftCol.Item().Text("☐ External Training");
                        leftCol.Item().Text("☐ On-job Training");
                        leftCol.Item().Text("☑ Internal Training");
                    });

                    row.RelativeItem().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                        });

                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Course / Name Training:").Bold();
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text(model.TrainingName ?? "N/A");

                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Instructor:").Bold();
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text(model.TrainerName ?? "N/A");

                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Commence Date:").Bold();
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text(model.TrainingDate.ToString("dd-MMM-yyyy"));

                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Location:").Bold();
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("N/A");
                    });
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80);
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                        columns.RelativeColumn();
                    });

                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Certification ?").Bold();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("☐");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Duration of Training:").Bold();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text(model.DurationDisplay ?? "N/A");

                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Requested by:").Bold();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("N/A");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Cost (include GST?):").Bold();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("N/A");

                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Approved by:").Bold();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Sign / Date:").Bold();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("");
                });

                col.Item().PaddingTop(5).Text("Participants List").Bold().Underline();
                
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(20);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyleHeader).Text("#").Bold();
                        header.Cell().Element(CellStyleHeader).Text("Employee Name").Bold();
                        header.Cell().Element(CellStyleHeader).Text("Employee ID").Bold();
                        header.Cell().Element(CellStyleHeader).Text("Dept").Bold();
                        header.Cell().Element(CellStyleHeader).Text("Signature").Bold();
                        header.Cell().Element(CellStyleHeader).Text("Remarks/Attendance").Bold();
                    });

                    int counter = 1;
                    var participantsList = model.Participants ?? new List<ParticipantUserVM>();
                    
                    foreach (var p in participantsList)
                    {
                        table.Cell().Element(CellStyle).Text(counter++.ToString());
                        table.Cell().Element(CellStyle).Text(p.EmployeeName ?? "-");
                        table.Cell().Element(CellStyle).Text(p.UserName ?? "-");
                        table.Cell().Element(CellStyle).Text(p.Department ?? "-");
                        table.Cell().Element(CellStyle).Text("");
                        table.Cell().Element(CellStyle).Text("");
                    }
                });
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }).GeneratePdf();

    static IContainer CellStyle(IContainer container) => container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3);
    static IContainer CellStyleHeader(IContainer container) => container.Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4).Padding(3).AlignCenter();
}

        

    }
}