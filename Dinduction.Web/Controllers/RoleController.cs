using Microsoft.AspNetCore.Mvc;
using Dinduction.Application.Interfaces;
using Dinduction.Web.Models;
using AutoMapper;
using Dinduction.Domain.Entities;

namespace Dinduction.Web.Controllers;

public class RoleController : Controller
{
    private readonly IRoleService _service;
    private readonly IMapper _mapper;

    public RoleController(IRoleService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

 
    public async Task<IActionResult> Index()
    {
        try
        {
            var roles = await _service.GetAllAsync();
            var model = _mapper.Map<List<RoleVM>>(roles);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new List<RoleVM>());
        }
    }

   
    public IActionResult Create()
    {
        return View();
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var entity = _mapper.Map<Role>(model);
            await _service.InsertAsync(entity);
            TempData["SuccessMessage"] = "Role berhasil ditambahkan.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // GET: Role/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var role = await _service.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<RoleVM>(role);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Role/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoleVM model)
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
            var entity = _mapper.Map<Role>(model);
            await _service.UpdateAsync(entity);
            TempData["SuccessMessage"] = "Role berhasil diperbarui.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // POST: Role/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "Role berhasil dihapus.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}