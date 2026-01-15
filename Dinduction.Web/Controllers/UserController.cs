using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Dinduction.Application.Interfaces;
using Dinduction.Infrastructure.Helpers;
using Dinduction.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _service;
        private readonly IRoleService _serviceRole;
        private readonly IMapper _mapper;

        public UserController(IUserService service, IRoleService serviceRole, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _serviceRole = serviceRole ?? throw new ArgumentNullException(nameof(serviceRole));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private async Task<List<SelectListItem>> GetRoleAsync()
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Select Role --" }
            };

            var roles = await _serviceRole.GetAllAsync();
            foreach (var r in roles)
            {
                list.Add(new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.RoleName
                });
            }

            return list;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _service.GetAdminTrainerAsync();
                var model = users.Select(u => _mapper.Map<UserVM>(u)).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<UserVM>());
            }
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            var model = new UserVM();
            try
            {
                model.ListRole = new SelectList(await GetRoleAsync(), "Value", "Text");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(model);
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                model.ListRole = new SelectList(await GetRoleAsync(), "Value", "Text");
                return View(model);
            }

            try
            {
                var entity = _mapper.Map<Dinduction.Domain.Entities.User>(model);
                // default password policy in old app: set password = username
                entity.Password = model.UserName;
                await _service.InsertAsync(entity);

                TempData["SuccessMessage"] = "User berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.ListRole = new SelectList(await GetRoleAsync(), "Value", "Text");
                return View(model);
            }
        }

        // GET: User/UserWeekly
        public async Task<IActionResult> UserWeekly()
        {
            try
            {
                var users = await _service.GetWeeklyAsync();
                var model = users.Select(u => _mapper.Map<UserVM>(u)).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<UserVM>());
            }
        }

        // GET: User/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: User/UploadUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadUser(IFormFile importexcelfile)
        {
            if (importexcelfile == null || importexcelfile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(UserWeekly));
            }

            try
            {
                using var stream = importexcelfile.OpenReadStream();
                await ExcelHelper.UploadUserAsync(stream, _service);
                TempData["SuccessMessage"] = "Upload berhasil.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(UserWeekly));
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
                return BadRequest();

            var user = await _service.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = _mapper.Map<UserVM>(user);
            return View(model);
        }

        // POST: User/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(UserVM model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                await _service.DeleteAsync(model.Id);
                TempData["SuccessMessage"] = "User berhasil dihapus.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}