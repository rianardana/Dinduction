using Microsoft.AspNetCore.Mvc;
using Dinduction.Application.Interfaces;
using Dinduction.Web.Models;
using System.Security.Claims;
using AutoMapper;
using Dinduction.Domain.Entities;
using Dinduction.Application.DTOs;

namespace Dinduction.Web.Controllers;

public class QuizController : Controller
{
    private readonly ITrainingService _trainingService;
    private readonly IRecordTrainingService _recordService;
    private readonly IUserService _userService;
    private readonly IQuestionService _questionService;
    private readonly IParticipantService _participantService;
    private readonly ITrainerService _trainerService;
    private readonly IMapper _mapper;

    public QuizController(ITrainingService trainingService,IRecordTrainingService recordService,IUserService userService,IQuestionService questionService,
        IParticipantService participantService,ITrainerService trainerService,IMapper mapper)
    {
        _trainingService = trainingService;
        _recordService = recordService;
        _userService = userService;
        _questionService = questionService;
        _participantService = participantService;
        _trainerService = trainerService;
        _mapper = mapper;
    }

    private int GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId") ?? 0;
    }

    // GET: Quiz
    public async Task<IActionResult> Index()
    {
        try
        {
            var trainings = await _trainingService.GetAllAsync();
            var model = _mapper.Map<List<MasterTrainingVM>>(trainings);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<MasterTrainingVM>());
        }
    }

    
    public async Task<IActionResult> Record(int id)
    {
        var model = new MasterTrainingVM();
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var training = await _trainingService.GetByIdAsync(id);
            var user = await _userService.GetUserNameByIdAsync(userId);
            var badge = await _userService.GetBadgeNumberByIdAsync(userId);
            var countQuestion = await _questionService.GetTotalQuestionAsync(id);
            
            var participantId = await _participantService.GetParticipantAsync(userId);
            //var lastQuizNo = await _recordService.GetLastQuizNoAsync(participantId, id);
          //  var quizStatus = await _recordService.GetQuizStatusAsync(id, participantId);
            
            bool exist = await _recordService.CheckExistingAsync(id, participantId);
            bool trainerAllowed = await _participantService.IsTrainerInputAsync(userId, id);

            model = _mapper.Map<MasterTrainingVM>(training);
            model.TrainingDate = DateTime.Now;
            model.EmployeeName = user;
            model.EmployeeNumber = "12345";
            model.QuizNo = 5;

            TempData["ParticipantId"] = participantId;
            TempData["QuestionCount"] = countQuestion;
           // TempData["QuizStatus"] = quizStatus.ToString();

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // GET: Quiz/Test?trainingId=5&Number=1&QuizNo=1
    public async Task<IActionResult> Test(int trainingId, int number, int quizNo)
    {
        if (number == 0) return BadRequest();

        var model = new ViewQuestionAnswerVM();
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var participantId = await _participantService.GetParticipantAsync(userId);
            var entity = await _questionService.GetQuestionByNumberAsync(trainingId, number);

            if (entity == null) return NotFound();

            model = _mapper.Map<ViewQuestionAnswerVM>(entity);
            model.Number = number;
            model.TrainingId = trainingId;
            model.QuizNumber = quizNo;

            var options = new List<AnswerOptionVM>
            {
                new AnswerOptionVM { Key = "RightAnswer", Text = model.RightAnswer, ImagePath = model.ImageRight },
                new AnswerOptionVM { Key = "OptionA", Text = model.OptionA, ImagePath = model.ImageA },
                new AnswerOptionVM { Key = "OptionB", Text = model.OptionB, ImagePath = model.ImageB },
                new AnswerOptionVM { Key = "OptionC", Text = model.OptionC, ImagePath = model.ImageC }
            };

            var rnd = new Random();
            model.ShuffledOptions = options.OrderBy(x => rnd.Next()).ToList();

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // POST: Quiz/Test
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Test(RecordTrainingVM model)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var participantId = await _participantService.GetParticipantAsync(userId);
            var questionId = model.Id;
            var trainingId = await _questionService.GetTrainingIdAsync(questionId);
            var totalQuestion = await _questionService.GetTotalQuestionAsync(trainingId);
            var quizNo = model.QuizNumber;
            var number = await _questionService.GetNumberAsync(questionId);
            var trainerId = await _participantService.GetTrainerIdByUserAndTrainingAsync(userId, trainingId);

            var exists = await _recordService.CheckAnsweredAsync(participantId, trainingId, quizNo, number);
            if (exists)
            {
                return RedirectToAction("Skip", new { trainingId = trainingId, currentNumber = number, quizNo = quizNo });
            }

            var entity = _mapper.Map<RecordTraining>(model);
            entity.ParticipantId = participantId;
            entity.QuestionId = questionId;
            entity.RecordDate = DateTime.Now;
            entity.UserAnswer = model.SelectedOption;
            entity.IsTrue = (entity.UserAnswer == await _questionService.GetTrueFalseAsync(model.Id));
            entity.NumberQuestion = number;
            entity.TrainingId = trainingId;
            entity.QuizNumber = quizNo;
            entity.TrainerId = trainerId;

            await _recordService.InsertAsync(entity);

            // Cari nomor berikutnya
            var answeredNumbers = await _recordService.GetAnswerAsync(participantId, trainingId, quizNo);
            int nextNumber = FindNextUnansweredNumber(number, totalQuestion, answeredNumbers);

            if (nextNumber == -1)
            {
                return RedirectToAction("Complete");
            }

            return RedirectToAction("Test", new { trainingId = trainingId, Number = nextNumber, QuizNo = quizNo });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // Helper method
    private int FindNextUnansweredNumber(int currentNumber, int totalQuestion, List<int> answeredNumbers)
    {
        for (int i = currentNumber + 1; i <= totalQuestion; i++)
        {
            if (!answeredNumbers.Contains(i))
            {
                return i;
            }
        }

        for (int i = 1; i <= totalQuestion; i++)
        {
            if (!answeredNumbers.Contains(i))
            {
                return i;
            }
        }

        return -1;
    }

    // GET: Quiz/Skip
    public async Task<IActionResult> Skip(int trainingId, int currentNumber, int quizNo)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var participantId = await _participantService.GetParticipantAsync(userId);
            var totalQuestion = await _questionService.GetTotalQuestionAsync(trainingId);
            var answeredNumbers = await _recordService.GetAnswerAsync(participantId, trainingId, quizNo);

            int nextNumber = FindNextUnansweredNumber(currentNumber, totalQuestion, answeredNumbers);

            if (nextNumber == -1)
            {
                return RedirectToAction("Complete");
            }

            return RedirectToAction("Test", new { trainingId = trainingId, Number = nextNumber, QuizNo = quizNo });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("Error");
        }
    }

    // GET: Quiz/Continue
    public async Task<IActionResult> Continue(int trainingId, int quizNo)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var participantId = await _participantService.GetParticipantAsync(userId);
            var totalQuestion = await _questionService.GetTotalQuestionAsync(trainingId);
            var answeredNumbers = await _recordService.GetAnswerAsync(participantId, trainingId, quizNo);

            int nextNumber = -1;
            for (int i = 1; i <= totalQuestion; i++)
            {
                if (!answeredNumbers.Contains(i))
                {
                    nextNumber = i;
                    break;
                }
            }

            if (nextNumber == -1)
            {
                return RedirectToAction("Complete");
            }

            return RedirectToAction("Test", new { trainingId = trainingId, Number = nextNumber, QuizNo = quizNo });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("Error");
        }
    }

    // GET: Quiz/Complete
    public async Task<IActionResult> Complete()
    {
        var model = new ViewRecordTrainingVM();
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var participantId = await _participantService.GetParticipantAsync(userId);
            var data = await _recordService.GetByParticipantIdAsync(participantId);
            
            if (data != null)
            {
                model = _mapper.Map<ViewRecordTrainingVM>(data);
            }

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        
    }

    [HttpPost]
    public async Task<JsonResult> DeleteRecord(int participantId, int trainingId, int quizNumber)
    {
        try
        {
            var deleted = await _recordService.DeleteRecordAsync(participantId, trainingId, quizNumber);

            if (deleted)
            {
                return Json(new { success = true, message = "Data berhasil dihapus." });
            }
            else
            {
                return Json(new { success = false, message = "Data tidak ditemukan atau gagal dihapus." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
        }
    }

    public async Task<IActionResult> MyResult()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var participantId = await _participantService.GetParticipantAsync(userId);
            var results = await _recordService.GetLatestResultForUserAsync(participantId);

            return View(results);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<UserTrainingDTO>());
        }
    }
    public IActionResult CheckResult()
    {
        return View();
    }

    
    public async Task<JsonResult> GetResults()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Json(new { success = false, message = "Not authenticated" });

            var participantId = await _participantService.GetParticipantAsync(userId);
            var records = await _recordService.GetLatestResultByParticipantAsync(participantId);
            
            var data = _mapper.Map<List<VRecordMaster>>(records); // atau langsung kirim records

            return Json(new
            {
                success = true,
                data = data
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
        }
    }

    public async Task<JsonResult> GetDetailsById(int participantId, int trainingId)
    {
        try
        {
            var score = await _recordService.GetLastScoreAsync(trainingId, participantId);
            return Json(new { data = score });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Terjadi kesalahan: " + ex.Message });
        }
    }
}