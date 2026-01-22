using Microsoft.AspNetCore.Mvc;
using Dinduction.Application.Interfaces;
using Dinduction.Web.Models;
using System.Security.Claims;
using AutoMapper;
using Dinduction.Domain.Entities;

namespace Dinduction.Web.Controllers;

public class QuestionController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ITrainingService _trainingService;
    private readonly IMapper _mapper;

    public QuestionController(IQuestionService questionService,ITrainingService trainingService,IMapper mapper)
    {
        _questionService = questionService;
        _trainingService = trainingService;
        _mapper = mapper;
    }

    // GET: Question
    public async Task<IActionResult> Index()
    {
        try
        {
            var questions = await _questionService.GetAllWithDetailsAsync();;
            var model = _mapper.Map<List<QuestionVM>>(questions);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<QuestionVM>());
        }
    }

    // GET: Question/Create/5
    public async Task<IActionResult> Create(int id)
    {
        var model = new QuestionVM
        {
            TrainingId = id,
            Answers = new List<AnswerVM> { new AnswerVM { Id = 1 } }
        };

        try
        {
            var lastNumber = await _questionService.GetLastNumberAsync();
            var training = await _trainingService.GetByIdAsync(id);

            model.Number = lastNumber;
            model.EvaluationForm = training?.EvaluationForm ?? string.Empty;
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        return View(model);
    }

    // POST: Question/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var entity = _mapper.Map<Question>(model);
            await _questionService.InsertAsync(entity);
            TempData["SuccessMessage"] = "Question created successfully.";
            return RedirectToAction("Index", "MasterTraining");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}