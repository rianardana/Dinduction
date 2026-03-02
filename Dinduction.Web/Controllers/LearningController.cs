using Microsoft.AspNetCore.Mvc;
using Dinduction.Application.Interfaces;
using Dinduction.Web.Models;
using AutoMapper;
using Dinduction.Domain.Entities;
using System.Linq;

namespace Dinduction.Web.Controllers;

public class LearningController : Controller
{
    private readonly ITrainingService _trainingService;
    private readonly ILearningMaterialService _materialService;
    private readonly IUserLearningProgressService _progressService;
    private readonly IUserService _userService;
    private readonly IParticipantService _participantService;
    private readonly IRecordTrainingService _recordService;
    private readonly IMapper _mapper;

    public LearningController(
        ITrainingService trainingService,
        ILearningMaterialService materialService,
        IUserLearningProgressService progressService,
        IUserService userService,
        IParticipantService participantService, IRecordTrainingService recordService,
        IMapper mapper)
    {
        _trainingService = trainingService;
        _materialService = materialService;
        _progressService = progressService;
        _userService = userService;
        _participantService = participantService;
        _mapper = mapper;
    }

    private int GetCurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

    // GET: Learning/Index
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: Learning/Study?trainingId=5
    public async Task<IActionResult> Study(int trainingId)
    {
    
        var model = new LearningStudyVM();
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var trainingType = HttpContext.Session.GetString("TrainingType") ?? "I";

    
                if (trainingType == "R")
        {
            try 
            {
                var participantId = await _participantService.GetParticipantAsync(userId);
                
                bool preTestCompleted;
                try
                {
                    preTestCompleted = await _recordService.IsQuizCompletedAsync(participantId, trainingId, 1);
                    
                }
                catch (Exception ex)
                {
                    
                    throw; 
                }
                
                if (!preTestCompleted)
                {
                    
                    return RedirectToAction("Record", "Quiz", new { id = trainingId, quizNo = 1 });
                }
            }
            catch (Exception ex)
            {
                TempData["Debug"] = $"ERROR di R-block: {ex.GetType().Name} - {ex.Message}";
                // JANGAN throw, biarkan lanjut ke bawah dulu
            }
        }

            var training = await _trainingService.GetByIdAsync(trainingId);
            var user = await _userService.GetUserNameByIdAsync(userId);
            var badge = await _userService.GetBadgeNumberByIdAsync(userId);
            
            var materials = await _materialService.GetByTrainingIdAsync(trainingId);
            var currentYear = DateTime.Now.Year;
            var userMaterials = new List<UserLearningMaterialVM>();
            foreach (var mat in materials)
            {
                bool isCompleted = false;
                try
                {
                    isCompleted = await _progressService.IsDoneAsync(userId, mat.Id, currentYear);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[IsDone ERROR] matId={mat.Id}: {ex.Message}");
                }

                userMaterials.Add(new UserLearningMaterialVM
                {
                    Id = mat.Id,
                    Title = string.IsNullOrEmpty(mat.FilePath) ? "Untitled" : Path.GetFileNameWithoutExtension(mat.FilePath),
                    FilePath = mat.FilePath,
                    IsCompleted = isCompleted
                });
            }

            model.TrainingId = trainingId;
            model.TrainingName = training?.TrainingName ?? "Training";
            model.EmployeeName = user ?? "User";
            model.EmployeeNumber = badge ?? "-";
            model.Materials = userMaterials;
            model.TotalMaterials = userMaterials.Count;
            model.CompletedMaterials = userMaterials.Count(m => m.IsCompleted);
            ViewBag.TrainingType = trainingType;
            ViewBag.TargetQuizNo = (trainingType == "R") ? 2 : 1;
            TempData["TrainingId"] = trainingId;

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    
            public async Task<IActionResult> ViewMaterial(int materialId, int trainingId)
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return RedirectToAction("Login", "Account");

                var material = await _materialService.GetByIdAsync(materialId);
                if (material == null)
                {
                    TempData["Error"] = "Material not found";
                    return RedirectToAction("Study", new { trainingId });
                }

                // ✅ DEBUG: Cek nilai FilePath dari database
                System.Diagnostics.Debug.WriteLine($"[ViewMaterial] DB FilePath: '{material.FilePath}'");

                // ✅ Safe path handling: pastikan FilePath valid
                string filePath;
                
                if (string.IsNullOrEmpty(material.FilePath))
                {
                    // ❌ Fallback kalau null/empty
                    filePath = null;
                }
                else if (material.FilePath.StartsWith("/study/"))
                {
                    // ✅ Sudah full path, pakai langsung
                    filePath = material.FilePath;
                }
                else
                {
                    // ✅ Hanya nama file, tambahkan prefix
                    filePath = "/study/" + material.FilePath;
                }

                System.Diagnostics.Debug.WriteLine($"[ViewMaterial] Final FilePath: '{filePath}'");

                var model = new UserLearningMaterialVM
                {
                    Id = material.Id,
                    Title = string.IsNullOrEmpty(filePath) ? "Untitled" : Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath
                };

                ViewBag.TrainingId = trainingId;
                return View(model);
            }

    // POST: Learning/MarkComplete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkComplete(int materialId, int trainingId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var currentYear = DateTime.Now.Year;
            await _progressService.MarkCompletedAsync(userId, materialId, currentYear);

            return RedirectToAction(nameof(Study), new { trainingId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction(nameof(Study), new { trainingId });
        }
    }

    // GET: Learning/Complete
    public async Task<IActionResult> Complete(int trainingId)
    {
        var model = new LearningCompleteVM();
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var training = await _trainingService.GetByIdAsync(trainingId);
            var user = await _userService.GetUserNameByIdAsync(userId);
            
            model.EmployeeName = user ?? "Peserta";
            model.TrainingName = training?.TrainingName ?? "Training";
            model.CompletedDate = DateTime.Now;
            model.NextStepUrl = Url.Action("Record", "Quiz", new { id = trainingId });

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }


       [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleteAndRedirect(int materialId, int trainingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return RedirectToAction("Login", "Account");

                var trainingType = HttpContext.Session.GetString("TrainingType") ?? "I";
                var currentYear = DateTime.Now.Year;
                
                await _progressService.MarkCompletedAsync(userId, materialId, currentYear);

                // ✅ Tentukan target quiz number
                int targetQuizNo = (trainingType == "R") ? 2 : 1;
                
                // ✅ KIRIM VIA TEMPDATA (Aman, gak ubah URL/Signature)
                TempData["TargetQuizNo"] = targetQuizNo;

                // ✅ Redirect biasa, gak perlu bawa param quizNo di URL
                return RedirectToAction("Record", "Quiz", new { id = trainingId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error MarkCompleteAndRedirect: {ex.Message}");
                
                var trainingType = HttpContext.Session.GetString("TrainingType") ?? "I";
                int targetQuizNo = (trainingType == "R") ? 2 : 1;
                
                TempData["TargetQuizNo"] = targetQuizNo;
                return RedirectToAction("Record", "Quiz", new { id = trainingId });
            }
        }

    
}