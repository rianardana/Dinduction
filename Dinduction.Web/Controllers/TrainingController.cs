using Microsoft.AspNetCore.Mvc;
using Dinduction.Application.Interfaces;
using Dinduction.Web.Models;
using AutoMapper;
using Dinduction.Domain.Entities;

namespace Dinduction.Web.Controllers;

public class TrainingController : Controller
{
    private readonly ITrainingService _service;
    private readonly IMapper _mapper;

    public TrainingController(ITrainingService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // GET: Training/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            var trainings = await _service.GetAllAsync();
            var model = _mapper.Map<List<MasterTrainingVM>>(trainings);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<MasterTrainingVM>());
        }
    }

    // GET: Training/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Training/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MasterTrainingVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var entity = _mapper.Map<MasterTraining>(model);
            await _service.InsertAsync(entity);
            TempData["SuccessMessage"] = "Training berhasil ditambahkan.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // GET: Training/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var training = await _service.GetByIdAsync(id);
            if (training == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<MasterTrainingVM>(training);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Training/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MasterTrainingVM model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var entity = _mapper.Map<MasterTraining>(model);
            await _service.UpdateAsync(entity);
            TempData["SuccessMessage"] = "Training berhasil diperbarui.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // POST: Training/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "Training berhasil dihapus.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}