using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Dinduction.Web.Models;
using Dinduction.Application.Interfaces;
using Dinduction.Application.Models;

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

        public ResultController(
            IRecordTrainingService recordTrainingService,
            IQuestionService questionService,
            IParticipantService participantService,
            IUserService userService,
            ITrainerService trainerService,
            ITrainingService masterTrainingService,
            IMapper mapper)
        {
            _recordTrainingService = recordTrainingService;
            _questionService = questionService;
            _participantService = participantService;
            _userService = userService;
            _trainerService = trainerService;
            _masterTrainingService = masterTrainingService;
            _mapper = mapper;
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
        public async Task<IActionResult> PrintPDF(int trainingId, int participantId)
        {
            if (trainingId == 0 || participantId == 0)
                return BadRequest();

            try
            {
                var resultFirst = await _questionService.GetDetailAsync(trainingId, participantId);
                var entity = await _recordTrainingService.GetResultAsync(trainingId, participantId);
                if (entity == null)
                    return NotFound();

                var score = await _recordTrainingService.GetScoreAsync(trainingId, participantId);
                var trainerId = await _recordTrainingService.GetTrainerAsync(participantId, trainingId);
                var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
                var trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);
                var answers = await _questionService.GetListAnswerAsync(trainingId, participantId);

                var model = _mapper.Map<ViewRecordResultVM>(entity);
                model.QuestionAnswers = _mapper.Map<List<ViewQuestionAnswerUserVM>>(answers);
                model.TrainerName = trainerName;
                model.Score = score;

                // Manual data mapping
                model.FormDateRegistration = resultFirst.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
                model.FormNumberRegistration = resultFirst.FormNumberRegistration;
                model.Purpose1 = resultFirst.Purpose1;
                model.Purpose2 = resultFirst.Purpose2;
                model.PurposeEnglish1 = resultFirst.PurposeEnglish1;
                model.PurposeEnglish2 = resultFirst.PurposeEnglish2;
                model.TrainingName = resultFirst.TrainingName;
                model.EvaluationForm = resultFirst.EvaluationForm;
                model.TrainingDate = resultFirst.RecordDate ?? DateTime.Now;

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
        // PRINT PDF - HISTORY RESULT
        // ============================================
        public async Task<IActionResult> PrintPDFHistory(int trainingId, int participantId, int quizNumber)
        {
            if (trainingId == 0 || participantId == 0 || quizNumber == 0)
                return BadRequest();

            try
            {
                var resultFirst = await _questionService.GetDetailAsync(trainingId, participantId);
                var entity = await _recordTrainingService.GetResultHistoryAsync(trainingId, participantId, quizNumber);
                if (entity == null)
                    return NotFound();

                var score = await _recordTrainingService.GetScoreHistoryAsync(trainingId, participantId, quizNumber);
                var trainerId = await _recordTrainingService.GetTrainerAsync(participantId, trainingId);
                var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
                var trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);
                var answers = await _questionService.GetListAnswerHistoryAsync(trainingId, participantId, quizNumber);

                var model = _mapper.Map<ViewRecordResultVM>(entity);
                model.QuestionAnswers = _mapper.Map<List<ViewQuestionAnswerUserVM>>(answers);
                model.TrainerName = trainerName;
                model.Score = score;

                // Manual data mapping
                model.FormDateRegistration = resultFirst.FormDateRegistration?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
                model.FormNumberRegistration = resultFirst.FormNumberRegistration;
                model.Purpose1 = resultFirst.Purpose1;
                model.Purpose2 = resultFirst.Purpose2;
                model.PurposeEnglish1 = resultFirst.PurposeEnglish1;
                model.PurposeEnglish2 = resultFirst.PurposeEnglish2;
                model.TrainingName = resultFirst.TrainingName;
                model.EvaluationForm = resultFirst.EvaluationForm;
                model.TrainingDate = resultFirst.RecordDate ?? DateTime.Now;

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
        // PDF GENERATION - CURRENT (Placeholder)
        // ============================================
        public async Task<IActionResult> PrintQuiz(int trainingId, int participantId)
        {
            // TODO: Implement PDF generation using QuestPDF or DinkToPdf
            // For now, redirect to PrintPDF view
            return RedirectToAction("PrintPDF", new { trainingId, participantId });
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
            try
            {
                var entities = await _masterTrainingService.GetAllAsync();
                var model = _mapper.Map<List<MasterTrainingVM>>(entities);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(new List<MasterTrainingVM>());
            }
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

        // ============================================
        // CUSTOM SERVER SIDE - TRAINER
        // ============================================
        // [HttpPost]
        // public async Task<JsonResult> CustomServerSide(DataTableAjaxPostModel model)
        // {
        //     try
        //     {
        //         var userId = GetCurrentUserId();
        //         var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                
        //         var (record, totalResultsCount) = await _recordTrainingService.SearchByTrainerAsync(model, trainerId);
        //         var sectionId = await _trainerService.GetSectionTrainerIdAsync(userId);
        //         var totalTrainingCount = await _trainerService.CountTrainingAsync(sectionId);

        //         if (record == null || !record.Any())
        //         {
        //             return Json(new
        //             {
        //                 draw = model.draw,
        //                 recordsTotal = 0,
        //                 recordsFiltered = 0,
        //                 data = new List<object>()
        //             });
        //         }

        //         var filteredResultsCount = record.Count;
        //         var participantIds = record.Where(c => c.ParticipantId.HasValue)
        //                                 .Select(c => c.ParticipantId.Value)
        //                                 .Distinct();

        //         // Get counts for each participant
        //         var completedTrainingCounts = new Dictionary<int, int>();
        //         var failedTrainingCounts = new Dictionary<int, int>();

        //         foreach (var participantId in participantIds)
        //         {
        //             var completedCount = await _recordTrainingService.CountCompletedAsync(participantId, trainerId);
        //             var failedCount = await _recordTrainingService.CountFailedAsync(participantId, trainerId);
                    
        //             completedTrainingCounts[participantId] = completedCount;
        //             failedTrainingCounts[participantId] = failedCount;
        //         }

        //         // Map data with counts
        //         var data = record.Select(c =>
        //         {
        //             var completedCount = c.ParticipantId.HasValue && completedTrainingCounts.ContainsKey(c.ParticipantId.Value)
        //                 ? completedTrainingCounts[c.ParticipantId.Value]
        //                 : 0;

        //             var failedCount = c.ParticipantId.HasValue && failedTrainingCounts.ContainsKey(c.ParticipantId.Value)
        //                 ? failedTrainingCounts[c.ParticipantId.Value]
        //                 : 0;

        //             // Create mapped model with additional parameters
        //             var mappedModel = _mapper.Map<ViewRecordMasterVM>(c);
        //             mappedModel.TotalTrainingCount = totalTrainingCount;
        //             mappedModel.CompletedTrainingCount = completedCount;
        //             mappedModel.Failed = failedCount;
                    
        //             return mappedModel;
        //         });

        //         return Json(new
        //         {
        //             draw = model.draw,
        //             recordsTotal = totalResultsCount,
        //             recordsFiltered = filteredResultsCount,
        //             data = data
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return Json(new
        //         {
        //             draw = model.draw,
        //             recordsTotal = 0,
        //             recordsFiltered = 0,
        //             data = new List<object>(),
        //             error = ex.Message
        //         });
        //     }
        // }

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
                                        .Distinct();

                var completedTrainingCounts = new Dictionary<int, int>();
                var failedTrainingCounts = new Dictionary<int, int>();

                foreach (var participantId in participantIds)
                {
                    var completedCount = await _recordTrainingService.CountCompletedForAdminAsync(participantId);
                    var failedCount = await _recordTrainingService.CountFailedForAdminAsync(participantId);
                    
                    completedTrainingCounts[participantId] = completedCount;
                    failedTrainingCounts[participantId] = failedCount;
                }

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
    }
}