using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Dinduction.Application.Models;
using Dinduction.Web.Models;
using Dinduction.Application.Interfaces;

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

        // ============================================
        // PRESENCE REPORTS - ADMIN
        // ============================================

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

        // ============================================
        // PRESENCE REPORTS - TRAINER
        // ============================================

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

        // ============================================
        // PREVIEW TRAINING FORM - TRAINER
        // ============================================

        public async Task<IActionResult> PreviewTrainingForm(DateTime? date, int? trainingId)
        {
            var model = new ParticipantUserVM();

            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);

                var selectedDate = date ?? DateTime.Today;

                var data = await _participantService.GetPresenceByTrainerAsync(selectedDate, trainingId ?? 0, trainerId);

                // Get training details
                var training = await _masterTrainingService.GetByIdAsync(trainingId ?? 0);

                model.TrainingName = training?.TrainingName ?? "N/A";
                model.TrainingDate = selectedDate.Date;
                model.TrainerName = await _userService.GetUserNameByIdAsync(userId);
                ViewBag.TrainingType = "Internal";

                model.Participants = _mapper.Map<List<ParticipantUserVM>>(data);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View("_TrainingFormPdf", model);
        }

        // ============================================
        // PREVIEW TRAINING FORM - ADMIN
        // ============================================

        public async Task<IActionResult> PreviewTrainingFormAdmin(DateTime? date, int? trainingId)
        {
            if (!date.HasValue || !trainingId.HasValue || trainingId == 0)
                return BadRequest();  // ← Changed from HttpStatusCodeResult

            try
            {
                var training = await _masterTrainingService.GetByIdAsync(trainingId.Value);
                if (training == null)
                    return NotFound();  // ← Changed from HttpNotFound

                var trainerId = await _participantService.GetTrainerByDateAndTrainingAsync(date.Value, trainingId.Value);
                var userTrainerId = await _trainerService.GetUserIdByTrainerIdAsync(trainerId);
                var trainerName = await _userService.GetUserNameByIdAsync(userTrainerId);

                var participants = await _participantService.GetPresenceAsync(date.Value.Date, trainingId.Value);
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
                return View("_TrainingFormPdf", new ParticipantUserVM
                {
                    TrainingName = "Error Loading Data",
                    Participants = new List<ParticipantUserVM>()
                });
            }
        }

        // ============================================
        // DOWNLOAD TRAINING FORM - TRAINER (PDF)
        // ============================================

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

                // TODO: Implement PDF generation
                return View("_TrainingFormPdf", model);
            }
            catch (Exception ex)
            {
                model.TrainingName = "Gagal Generate PDF";
                model.Participants = new List<ParticipantUserVM>();
                return View("_TrainingFormPdf", model);
            }
        }

        // ============================================
        // DOWNLOAD TRAINING FORM - ADMIN (PDF)
        // ============================================

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

                // TODO: Implement PDF generation
                return View("_TrainingFormPdf", model);
            }
            catch (Exception ex)
            {
                model.TrainingName = "Gagal Generate PDF";
                model.Participants = new List<ParticipantUserVM>();
                return View("_TrainingFormPdf", model);
            }
        }

        // ============================================
        // AJAX - GET PRESENCE DATA - ADMIN
        // ============================================

        public async Task<JsonResult> GetPresenceData(string date, int? trainingId)
        {
            if (trainingId == null || trainingId == 0)
                return Json(new List<object>());

            if (!DateTime.TryParse(date, out var selectedDate))
                return Json(new List<object>());

            try
            {
                var data = await _participantService.GetPresenceAsync(selectedDate, trainingId.Value);
                var mappedData = _mapper.Map<List<ParticipantUserVM>>(data);

                return Json(mappedData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

       // ============================================
        // AJAX - GET PRESENCE DATA - TRAINER
        // ============================================

        public async Task<IActionResult> GetPresenceDataByTrainer(string date, int? trainingId)
        {
            if (trainingId == null || trainingId == 0)
                return Ok(new List<object>());

            if (!DateTime.TryParse(date, out var selectedDate))
                return Ok(new List<object>());

            try
            {
                var userId = GetCurrentUserId();
                Console.WriteLine($"🔍 GetPresenceDataByTrainer - UserId: {userId}");
                
                if (userId == 0)
                {
                    Console.WriteLine("⚠️ UserId is 0");
                    return Ok(new List<object>());
                }

                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                Console.WriteLine($"🔍 TrainerId: {trainerId}, Date: {selectedDate:yyyy-MM-dd}, TrainingId: {trainingId}");

                var data = await _participantService.GetPresenceByTrainerAsync(selectedDate, trainingId.Value, trainerId);
                var mappedData = _mapper.Map<List<ParticipantUserVM>>(data);

                Console.WriteLine($"✅ Returning {mappedData.Count} attendance records");

                return Ok(mappedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetPresenceDataByTrainer: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return Ok(new List<object>());
            }
        }

        // ============================================
        // AJAX - GET TRAINING DATES - ADMIN
        // ============================================

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

        // ============================================
        // AJAX - GET TRAINING DATES - TRAINER
        // ============================================

        public async Task<IActionResult> GetTrainingDatesByTrainer()
        {
            try
            {
                var userId = GetCurrentUserId();
                var trainerId = await _trainerService.GetTrainerIdAsync(userId);
                var listDate = await _participantService.GetTrainingDatesByTrainerAsync(trainerId);
                var dates = listDate.Select(d => d.ToString("yyyy-MM-dd")).ToList();

                return Ok(dates); // ✅ Pakai Ok() biar bisa GET request
            }
            catch (Exception ex)
            {
                
                return Ok(new List<string>()); // ✅ Return empty list
            }
        }

        // ============================================
        // AJAX - GET TRAININGS BY DATE - ADMIN
        // ============================================

        public async Task<JsonResult> GetTrainingsByDate(string date)
        {
            if (!DateTime.TryParse(date, out var targetDate))
                return Json(new List<object>());

            try
            {
                var groupedTrainings = await _participantService.GetTrainingGroupedByDateAsync();
                var trainings = groupedTrainings
                    .Where(t => t.Date == targetDate.Date)
                    .SelectMany(t => t.Trainings)
                    .Select(tr => new
                    {
                        Value = tr.TrainingId,
                        Text = tr.TrainingName
                    })
                    .ToList();

                return Json(trainings);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, data = new List<object>() });
            }
        }

        // ============================================
        // AJAX - GET TRAININGS BY DATE - TRAINER
        // ============================================

       // ============================================
// AJAX - GET TRAININGS BY DATE - TRAINER
// ============================================

        public async Task<IActionResult> GetTrainingsByDateByTrainer(string date)
        {
            if (!DateTime.TryParse(date, out var targetDate))
                return Ok(new List<object>());

            try
            {
                var userId = GetCurrentUserId();
            
                
                if (userId == 0)
                {
                    Console.WriteLine("⚠️ UserId is 0, returning empty list");
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
    }
}