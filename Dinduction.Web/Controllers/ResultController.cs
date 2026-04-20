using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Dinduction.Web.Models;
using Dinduction.Application.Interfaces;
using Dinduction.Application.Models;
using Dinduction.Web.Services.Pdf.Interfaces;

namespace Dinduction.Web.Controllers
{
    public class ResultController : Controller
    {
        private readonly IRecordTrainingService _recordTrainingService;
        private readonly IQuestionService _questionService;
        private readonly IParticipantService _participantService;
        private readonly IUserService _userService;
        private readonly ITrainerService _trainerService;
        private readonly ITrainingService _masterTrainingService;
        private readonly IMapper _mapper;
         private readonly IPdfGeneratorService _pdfGenerator;

        public ResultController(
            IRecordTrainingService recordTrainingService,
            IQuestionService questionService,
            IParticipantService participantService,
            IUserService userService,
            ITrainerService trainerService,
            ITrainingService masterTrainingService,
            IMapper mapper,
            IPdfGeneratorService pdfGenerator)
        {
            _recordTrainingService = recordTrainingService;
            _questionService = questionService;
            _participantService = participantService;
            _userService = userService;
            _trainerService = trainerService;
            _masterTrainingService = masterTrainingService;
            _mapper = mapper;
            _pdfGenerator = pdfGenerator;
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // ============================================
        // INDEX - VIEW RESULT
        // ============================================
        public async Task<IActionResult> Index(int trainingId, int participantId)
        {
            if (trainingId == 0 || participantId == 0)
                return BadRequest();

            try
            {
                var entity = await _recordTrainingService.GetResultAsync(trainingId, participantId);
                if (entity == null)
                    return NotFound();

                var score = await _recordTrainingService.GetScoreAsync(trainingId, participantId);
                var trainerId = await _recordTrainingService.GetTrainerAsync(participantId, trainingId);
                var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
                var trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);
                var answers = await _questionService.GetLastListAnswerAsync(trainingId, participantId);
                
                var model = _mapper.Map<ViewRecordResultVM>(entity);
                model.QuestionAnswers = _mapper.Map<List<ViewQuestionAnswerUserVM>>(answers);
                model.TrainerName = trainerName;
                model.Score = score;
                
                ViewBag.PassFailed = score >= 80;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(new ViewRecordResultVM());
            }
        }

        // ============================================
        // PRINT PDF - CURRENT RESULT
        // ============================================
        // public async Task<IActionResult> PrintPDF(int trainingId, int participantId)
        // {
        //     if (trainingId == 0 || participantId == 0)
        //         return BadRequest();

        //     try
        //     {
        //         var resultFirst = await _questionService.GetDetailAsync(trainingId, participantId);
        //         var entity = await _recordTrainingService.GetResultAsync(trainingId, participantId);
        //         if (entity == null)
        //             return NotFound();

        //         var score = await _recordTrainingService.GetScoreAsync(trainingId, participantId);
        //         var trainerId = await _recordTrainingService.GetTrainerAsync(participantId, trainingId);
        //         var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
        //         var trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);
        //         var answers = await _questionService.GetListAnswerAsync(trainingId, participantId);

        //         var model = _mapper.Map<ViewRecordResultVM>(entity);
        //         model.QuestionAnswers = _mapper.Map<List<ViewQuestionAnswerUserVM>>(answers);
        //         model.TrainerName = trainerName;
        //         model.Score = score;

        //         // Manual data mapping
        //         model.FormDateRegistration = resultFirst.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
        //         model.FormNumberRegistration = resultFirst.FormNumberRegistration;
        //         model.Purpose1 = resultFirst.Purpose1;
        //         model.Purpose2 = resultFirst.Purpose2;
        //         model.PurposeEnglish1 = resultFirst.PurposeEnglish1;
        //         model.PurposeEnglish2 = resultFirst.PurposeEnglish2;
        //         model.TrainingName = resultFirst.TrainingName;
        //         model.EvaluationForm = resultFirst.EvaluationForm;
        //         model.TrainingDate = resultFirst.RecordDate ?? DateTime.Now;

        //         ViewBag.PassFailed = score >= 80;

        //         return View(model);
        //     }
        //     catch (Exception ex)
        //     {
        //         ModelState.AddModelError("", ex.Message);
        //         return View(new ViewRecordResultVM());
        //     }
        // }

        // ============================================
        // PRINT PDF - HISTORY RESULT
        // ============================================
        // ============================================
// PRINT PDF - HISTORY RESULT (FIXED)
// ============================================
public async Task<IActionResult> PrintPDFHistory(int trainingId, int participantId, int quizNumber)
{
    if (trainingId == 0 || participantId == 0 || quizNumber == 0)
        return BadRequest();

    try
    {
        // ✅ 1. Ambil Metadata Training dari MasterTraining
        var training = await _masterTrainingService.GetByIdAsync(trainingId);
        if (training == null) return NotFound("Training configuration not found.");

        // ✅ 2. Ambil Result History & Answers
        var entity = await _recordTrainingService.GetResultHistoryAsync(trainingId, participantId, quizNumber);
        if (entity == null) return NotFound("Result history not found.");

        var answers = await _questionService.GetListAnswerHistoryAsync(trainingId, participantId, quizNumber);
        var score = await _recordTrainingService.GetScoreHistoryAsync(trainingId, participantId, quizNumber);

        // ✅ 3. Mapping Utama
        var model = _mapper.Map<ViewRecordResultVM>(entity);
        model.QuestionAnswers = _mapper.Map<List<ViewQuestionAnswerUserVM>>(answers);
        model.TrainerName = "N/A"; // Optional: fetch trainer if needed
        model.Score = score;

        // ✅ 4. SAFE METADATA MAPPING (DARI MasterTraining)
        model.TrainingName = training.TrainingName ?? "Unknown Training";
        model.EvaluationForm = training.EvaluationForm ?? "-";
        model.Purpose1 = training.Purpose1 ?? "-";
        model.Purpose2 = training.Purpose2 ?? "-";
        model.PurposeEnglish1 = training.PurposeEnglish1 ?? "-";
        model.PurposeEnglish2 = training.PurposeEnglish2 ?? "-";
        model.FormNumberRegistration = training.FormNumberRegistration ?? "N/A";

        // ✅ 5. SAFE DATE CONVERSION
        model.FormDateRegistration = training.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
        model.TrainingDate = training.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;

        ViewBag.PassFailed = score >= 80;

        return View(model);
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", ex.Message);
        return View(new ViewRecordResultVM());
    }
}

     

        // ============================================
        // PDF GENERATION - HISTORY (Placeholder)
        // ============================================
        public async Task<IActionResult> PrintQuizHistory(int trainingId, int participantId, int quizNumber)
        {
            // TODO: Implement PDF generation using QuestPDF or DinkToPdf
            // For now, redirect to PrintPDFHistory view
            return RedirectToAction("PrintPDFHistory", new { trainingId, participantId, quizNumber });
        }

        // ============================================
        // PARTICIPANT RESULT LIST
        // ============================================
        public async Task<IActionResult> ParticipantResult()
        {
            return View();
        }

        // ============================================
        // MY PARTICIPANT RESULT
        // ============================================
        public async Task<IActionResult> MyParticipantResult()
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                ViewBag.TrainerId = trainerId;
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // ============================================
        // ALL RESULT VIEWS
        // ============================================
        public IActionResult AllResult()
        {
            return View();
        }

        public IActionResult AllHistory()
        {
            return View();
        }

        public IActionResult FailedHistory()
        {
            return View();
        }

    

        [HttpPost]
        public async Task<JsonResult> CustomServerSide(DataTableAjaxPostModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                
                var (record, totalResultsCount) = await _recordTrainingService.SearchByTrainerAsync(model, trainerId);
                var sectionId = await _trainerService.GetSectionTrainerIdAsync(userId);
                var totalTrainingCount = await _trainerService.CountTrainingAsync(sectionId);

                if (record == null || !record.Any())
                {
                    return Json(new
                    {
                        draw = model.draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>()
                    });
                }

                var filteredResultsCount = record.Count;
                var participantIds = record.Where(c => c.ParticipantId.HasValue)
                                        .Select(c => c.ParticipantId.Value)
                                        .Distinct()
                                        .ToList(); // ✅ Materialisasi dulu

                // ✅ BATCH QUERY - semua participant sekaligus
                var completedTrainingCounts = await _recordTrainingService.CountCompletedBatchAsync(participantIds, trainerId);
                var failedTrainingCounts = await _recordTrainingService.CountFailedBatchAsync(participantIds, trainerId);

                // Map data dengan counts
                var data = record.Select(c =>
                {
                    var completedCount = c.ParticipantId.HasValue && completedTrainingCounts.ContainsKey(c.ParticipantId.Value)
                        ? completedTrainingCounts[c.ParticipantId.Value]
                        : 0;

                    var failedCount = c.ParticipantId.HasValue && failedTrainingCounts.ContainsKey(c.ParticipantId.Value)
                        ? failedTrainingCounts[c.ParticipantId.Value]
                        : 0;

                    var mappedModel = _mapper.Map<ViewRecordMasterVM>(c);
                    mappedModel.TotalTrainingCount = totalTrainingCount;
                    mappedModel.CompletedTrainingCount = completedCount;
                    mappedModel.Failed = failedCount;
                    
                    return mappedModel;
                });

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = filteredResultsCount,
                    data = data
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
        // ============================================
        // CUSTOM SERVER SIDE - ADMIN
        // ============================================
        [HttpPost]
        public async Task<JsonResult> CustomServerSideforAdmin(DataTableAjaxPostModel model)
        {
            try
            {
                var (record, totalResultsCount) = await _recordTrainingService.SearchForAdminAsync(model);
                var totalTrainingCount = await _trainerService.CountTrainingForAdminAsync();

                if (record == null || !record.Any())
                {
                    return Json(new
                    {
                        draw = model.draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>()
                    });
                }

                var filteredResultsCount = record.Count;
                var participantIds = record.Where(c => c.ParticipantId.HasValue)
                                        .Select(c => c.ParticipantId.Value)
                                        .Distinct()
                                        .ToList(); 

                var completedTrainingCounts = await _recordTrainingService.CountCompletedBatchForAdminAsync(participantIds);
                var failedTrainingCounts = await _recordTrainingService.CountFailedBatchForAdminAsync(participantIds);

                
                var data = record.Select(c =>
                {
                    var completedCount = c.ParticipantId.HasValue && completedTrainingCounts.ContainsKey(c.ParticipantId.Value)
                        ? completedTrainingCounts[c.ParticipantId.Value]
                        : 0;

                    var failedCount = c.ParticipantId.HasValue && failedTrainingCounts.ContainsKey(c.ParticipantId.Value)
                        ? failedTrainingCounts[c.ParticipantId.Value]
                        : 0;

                    var mappedModel = _mapper.Map<ViewRecordMasterVM>(c);
                    mappedModel.TotalTrainingCount = totalTrainingCount;
                    mappedModel.CompletedTrainingCount = completedCount;
                    mappedModel.Failed = failedCount;
                    
                    return mappedModel;
                });

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = filteredResultsCount,
                    data = data
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

        // ============================================
        // CUSTOM SERVER SIDE - FAILED
        // ============================================
        [HttpPost]
        public async Task<JsonResult> CustomServerSideFailed(DataTableAjaxPostModel model)
        {
            try
            {
                var (record, totalResultsCount) = await _recordTrainingService.SearchFailedAsync(model);
                var totalTrainingCount = await _trainerService.CountTrainingForAdminAsync();

                var data = record.Select(async c =>
                {
                    if (c.TrainingId.HasValue && c.ParticipantId.HasValue)
                    {
                        var score = await _recordTrainingService.GetScoreHistoryAsync(
                            c.TrainingId.Value, 
                            c.ParticipantId.Value, 
                            1
                        );
                        
                        var mappedModel = _mapper.Map<ViewRecordMasterVM>(c);
                        mappedModel.Score = score;
                        mappedModel.TotalTrainingCount = totalTrainingCount;
                        
                        return mappedModel;
                    }
                    
                    return _mapper.Map<ViewRecordMasterVM>(c);
                });

                // Wait for all async operations
                var resolvedData = await Task.WhenAll(data);

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = 0,
                    data = resolvedData
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

        // ============================================
        // GET DETAILS BY ID - TRAINER
        // ============================================
        // ✅ SESUAI LEGACY - hanya participantId dan trainerId
        public async Task<JsonResult> GetDetailsById(int participantId, int trainerId)
        {
            try
            {
                var lastResults = await _recordTrainingService.GetLastResultByIdAsync(participantId, trainerId);
                var totalTrainingCount = await _trainerService.CountTrainingAsync(trainerId);

                var resultWithScore = new List<object>();

                foreach (var record in lastResults)
                {
                    if (record.TrainingId.HasValue)
                    {
                        var score = await _recordTrainingService.GetScoreAsync(record.TrainingId.Value, participantId);
                        
                        resultWithScore.Add(new
                        {
                            TrainingName = record.TrainingName,
                            Score = score,
                            TrainingId = record.TrainingId,
                            ParticipantId = participantId,
                            QuizNumber = record.QuizNumber,
                            TotalTrainingCount = totalTrainingCount
                        });
                    }
                }

                return Json(new { data = resultWithScore });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, data = new List<object>() });
            }
        }

        // ============================================
        // GET DETAILS FOR ADMIN
        // ============================================
        public async Task<JsonResult> GetDetailsforAdmin(int participantId)
        {
            try
            {
                var lastResults = await _recordTrainingService.GetLastResultForAdminAsync(participantId);
                var totalTrainingCount = await _trainerService.CountTrainingForAdminAsync();

                var resultWithScore = new List<object>();

                foreach (var record in lastResults)
                {
                    if (record.TrainingId.HasValue)
                    {
                        var score = await _recordTrainingService.GetScoreAsync(record.TrainingId.Value, participantId);
                        
                        resultWithScore.Add(new
                        {
                            TrainingName = record.TrainingName,
                            Score = score,
                            TrainingId = record.TrainingId,
                            TotalTrainingCount = totalTrainingCount,
                            ParticipantId = participantId
                        });
                    }
                }

                return Json(new { data = resultWithScore });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, data = new List<object>() });
            }
        }

        // ============================================
        // GET DETAILS HISTORY
        // ============================================
        public async Task<JsonResult> GetDetailsHistory(int participantId)
        {
            try
            {
                var lastResults = await _recordTrainingService.GetHistoryAsync(participantId);
                var totalTrainingCount = await _trainerService.CountTrainingForAdminAsync();

                var resultWithScore = new List<object>();

                foreach (var record in lastResults)
                {
                    if (record.TrainingId.HasValue && record.QuizNumber.HasValue)
                    {
                        var score = await _recordTrainingService.GetScoreHistoryAsync(
                            record.TrainingId.Value, 
                            participantId, 
                            record.QuizNumber.Value
                        );
                        
                        resultWithScore.Add(new
                        {
                            TrainingName = record.TrainingName,
                            Score = score,
                            TrainingId = record.TrainingId,
                            TotalTrainingCount = totalTrainingCount,
                            ParticipantId = participantId,
                            QuizNumber = record.QuizNumber
                        });
                    }
                }

                return Json(new { data = resultWithScore });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, data = new List<object>() });
            }
        }

    

        public async Task<IActionResult> MyRefreshResult()
        {
            var userId = GetCurrentUserId();
            var trainerId = await _trainerService.GetTrainerIdAsync(userId);
            ViewBag.TrainerId = trainerId;
            return View();
        }

        public async Task<IActionResult> RefreshResult()
        {
        
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> RefreshServerSide(DataTableAjaxPostModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                var (data, totalCount) = await _recordTrainingService.SearchRefreshByTrainerAsync(model, trainerId);
                var mapped = _mapper.Map<List<RefreshComparisonVM>>(data);

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = mapped
                });
            }
            catch (Exception ex)
            {
                return Json(new { draw = model.draw, recordsTotal = 0, 
                                recordsFiltered = 0, data = new List<object>(), 
                                error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RefreshServerSideForAdmin(DataTableAjaxPostModel model)
        {
            try
            {
                
                var (data, totalCount) = await _recordTrainingService.SearchRefreshByAdminAsync(model);
                var mapped = _mapper.Map<List<RefreshComparisonVM>>(data);

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = mapped
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

        [HttpGet]
        public async Task<JsonResult> GetRefreshDetails(int participantId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                var data = await _recordTrainingService.GetRefreshDetailsByParticipantAsync(participantId, trainerId);
                var mapped = _mapper.Map<List<RefreshDetailVM>>(data);

                return Json(new { success = true, data = mapped });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

       [HttpGet]
public async Task<IActionResult> PrintQuiz(int trainingId, int participantId)
{
    if (trainingId == 0 || participantId == 0) return BadRequest();

    try
    {
        var training = await _masterTrainingService.GetByIdAsync(trainingId);
        if (training == null) return NotFound("Training configuration not found.");

        var entity = await _recordTrainingService.GetResultAsync(trainingId, participantId);
        if (entity == null) return NotFound("Result data not found.");

        var answers = await _questionService.GetListAnswerAsync(trainingId, participantId);
        var score = await _recordTrainingService.GetScoreAsync(trainingId, participantId);

        var model = _mapper.Map<ViewRecordResultVM>(entity);
        model.QuestionAnswers = _mapper.Map<List<ViewQuestionAnswerUserVM>>(answers);
        model.Score = score;

        var trainerId = await _recordTrainingService.GetTrainerAsync(participantId, trainingId);
        var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
        model.TrainerName = await _userService.GetUserNameByIdAsync(userTrainerId);

        // ✅ SAFE METADATA MAPPING
        model.TrainingName = training.TrainingName ?? "Unknown Training";
        model.EvaluationForm = training.EvaluationForm ?? "-";
        model.Purpose1 = training.Purpose1 ?? "-";
        model.Purpose2 = training.Purpose2 ?? "-";
        model.PurposeEnglish1 = training.PurposeEnglish1 ?? "-";
        model.PurposeEnglish2 = training.PurposeEnglish2 ?? "-";
        model.FormNumberRegistration = training.FormNumberRegistration ?? "N/A";

        model.FormDateRegistration = training.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
        model.TrainingDate = training.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;

        ViewBag.PassFailed = score >= 80;

        var pdfBytes = await _pdfGenerator.GenerateTrainingResultPdfAsync(model);
        return File(pdfBytes, "application/pdf", $"Result_{training.TrainingName}_{model.EmployeeName}.pdf");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[PDF] ERROR: {ex.Message}\n{ex.StackTrace}");
        return BadRequest($"Failed to generate PDF: {ex.Message}");
    }
}
        
        public async Task<IActionResult> PrintPDF(int trainingId, int participantId)
{
    if (trainingId == 0 || participantId == 0)
        return BadRequest();

    try
    {
        // 1️⃣ Ambil Metadata Training (Purpose, Form No, Dates ada di sini)
        var training = await _masterTrainingService.GetByIdAsync(trainingId);
        if (training == null) return NotFound("Training configuration not found.");

        // 2️⃣ Ambil Result & Answers
        var entity = await _recordTrainingService.GetResultAsync(trainingId, participantId);
        if (entity == null) return NotFound("Result data not found.");
        
        var answers = await _questionService.GetListAnswerAsync(trainingId, participantId);
        var score = await _recordTrainingService.GetScoreAsync(trainingId, participantId);

        // 3️⃣ Mapping Utama
        var model = _mapper.Map<ViewRecordResultVM>(entity);
        model.QuestionAnswers = _mapper.Map<List<ViewQuestionAnswerUserVM>>(answers);
        model.Score = score;

        // 4️⃣ Trainer Info
        var trainerId = await _recordTrainingService.GetTrainerAsync(participantId, trainingId);
        var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
        model.TrainerName = await _userService.GetUserNameByIdAsync(userTrainerId);

        // 5️⃣ ✅ SAFE METADATA MAPPING (DARI MasterTraining, BUKAN VQuestionAnswerUser)
        model.TrainingName = training.TrainingName ?? "Unknown Training";
        model.EvaluationForm = training.EvaluationForm ?? "-";
        model.Purpose1 = training.Purpose1 ?? "-";
        model.Purpose2 = training.Purpose2 ?? "-";
        model.PurposeEnglish1 = training.PurposeEnglish1 ?? "-";
        model.PurposeEnglish2 = training.PurposeEnglish2 ?? "-";
        model.FormNumberRegistration = training.FormNumberRegistration ?? "N/A";

        // ✅ SAFE DATE CONVERSION (DateOnly? di DB → DateTime? di VM)
        model.FormDateRegistration = training.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
        model.TrainingDate = training.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;

        ViewBag.PassFailed = score >= 80;
        return View(model);
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", ex.Message);
        return View(new ViewRecordResultVM());
    }
}


    }
}