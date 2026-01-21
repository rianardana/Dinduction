using Microsoft.AspNetCore.Mvc;
using Dinduction.Application.Interfaces;
using Dinduction.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;
using Dinduction.Domain.Entities;

namespace Dinduction.Web.Controllers;

public class ParticipantController : Controller
{
    private readonly IUserService _userService;
    private readonly IParticipantService _participantService;
    private readonly ITrainerService _trainerService;
    private readonly ITrainingService _trainingService;
    private readonly IMapper _mapper;

    public ParticipantController(
        IUserService userService,
        IParticipantService participantService,
        ITrainerService trainerService,
        ITrainingService trainingService,
        IMapper mapper)
    {
        _userService = userService;
        _participantService = participantService;
        _trainerService = trainerService;
        _trainingService = trainingService;
        _mapper = mapper;
    }

    // Helper: Get current user ID from claims
    private int GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId") ?? 0;
    }

    // GET: Participant
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var trainerId = await _trainerService.GetTrainerIdAsync(userId);
            var participants = await _participantService.GetUsersAsync(trainerId);
            var model = _mapper.Map<List<ParticipantUserVM>>(participants);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<ParticipantUserVM>());
        }
    }

    // GET: Participant/ChooseTraining
    public async Task<IActionResult> ChooseTraining()
    {
        try
        {
            var userId = GetCurrentUserId();
            var sectionId = await _trainerService.GetSectionTrainerIdAsync(userId);
            var trainings = await _trainingService.GetByTrainerAsync(sectionId); 
            var model = _mapper.Map<List<MasterTrainingVM>>(trainings);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<MasterTrainingVM>());
        }
    }

    // GET: Participant/Select/5
    public async Task<IActionResult> Select(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var trainer = await _trainerService.GetByUserIdAsync(userId);
            if (trainer == null)
            {
                ModelState.AddModelError("", "Anda tidak terdaftar sebagai trainer.");
                return View(new ParticipantUserVM { Id = id });
            }

            var trainingName = await _trainingService.GetTrainingNameAsync(id);

            var model = new ParticipantUserVM
            {
                Id = id,
                TrainingName = trainingName,
                TrainerName = trainer.User?.EmployeeName ?? "Unknown",
                SectionTrainerId = trainer.SectionId.Value,
                ListParticipant = await GetEmployeeAsync(id, trainer.SectionId.Value),
                SelectedParticipants = new List<int>()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new ParticipantUserVM { Id = id });
        }
    }
    // POST: Participant/Select
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Select(ParticipantUserVM model)
    {
        if (!ModelState.IsValid)
        {
            model.ListParticipant = await GetEmployeeAsync(model.Id, model.SectionTrainerId);
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var trainer = await _trainerService.GetByUserIdAsync(userId);
            if (trainer == null)
            {
                ModelState.AddModelError("", "Anda tidak terdaftar sebagai trainer.");
                model.ListParticipant = await GetEmployeeAsync(model.Id, model.SectionTrainerId);
                return View(model);
            }

            var date = DateTime.Now;

            foreach (var selectedUserId in model.SelectedParticipants)
            {
                var entity = new ParticipantUser
                {
                    TrainerId = trainer.Id, 
                    TrainingDate = date,
                    UserId = selectedUserId,
                    TrainingId = model.Id,
                    SectionTrainerId = trainer.SectionId
                };

                await _participantService.InsertAsync(entity);
            }

            TempData["SuccessMessage"] = "Peserta berhasil ditambahkan.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.ListParticipant = await GetEmployeeAsync(model.Id, model.SectionTrainerId);
            return View(model);
        }
    }
    // Helper: Get available employees
    private async Task<List<SelectListItem>> GetEmployeeAsync(int trainingId, int sectionId)
    {
        var list = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "-- Pilih Peserta --" }
        };

        var allParticipants = await _userService.GetListParticipantAsync();
        foreach (var user in allParticipants)
        {
            var exists = await _participantService.IsExistAsync(user.Id, sectionId, trainingId);
            if (!exists)
            {
                list.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = $"{user.UserName} - {user.EmployeeName}"
                });
            }
        }

        return list;
    }
}