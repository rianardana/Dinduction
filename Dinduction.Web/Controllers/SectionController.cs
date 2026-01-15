using Microsoft.AspNetCore.Mvc;
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using Dinduction.Web.Models;
using AutoMapper;

namespace Dinduction.Web.Controllers;

public class SectionController : Controller
{
    private readonly ISectionService _service;
    private readonly IMapper _mapper;

    public SectionController(ISectionService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // GET: Section/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            var sections = await _service.GetAllAsync();
            var model = _mapper.Map<List<SectionVM>>(sections);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<SectionVM>());
        }
    }

    // GET: Section/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Section/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SectionVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var entity = _mapper.Map<Section>(model);
            await _service.InsertAsync(entity);
            TempData["SuccessMessage"] = "Section berhasil ditambahkan.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // GET: Section/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var section = await _service.GetByIdAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<SectionVM>(section);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Section/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SectionVM model)
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
            var entity = _mapper.Map<Section>(model);
            await _service.UpdateAsync(entity);
            TempData["SuccessMessage"] = "Section berhasil diperbarui.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // POST: Section/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "Section berhasil dihapus.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}