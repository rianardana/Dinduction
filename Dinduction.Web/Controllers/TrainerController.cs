using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Dinduction.Application.Interfaces;
using Dinduction.Domain.Entities;
using Dinduction.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Controllers
{
    public class TrainerController : Controller
    {
        private readonly ITrainerService _service;
        private readonly ISectionService _serviceSection;
        private readonly IUserService _serviceUser;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public TrainerController(ITrainerService service,ISectionService serviceSection,IUserService serviceUser,IMapper mapper,IWebHostEnvironment env)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _serviceSection = serviceSection ?? throw new ArgumentNullException(nameof(serviceSection));
            _serviceUser = serviceUser ?? throw new ArgumentNullException(nameof(serviceUser));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // Helper: sections select list
        private async Task<List<SelectListItem>> GetSectionAsync()
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Select Section --" }
            };

            var sections = await _serviceSection.GetAllAsync();
            foreach (var s in sections)
            {
                list.Add(new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SectionName
                });
            }

            return list;
        }

        // Helper: trainer/users select list
        private async Task<List<SelectListItem>> GetTrainerAsync()
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Select Trainer --" }
            };

            // I'm assuming your IUserService exposes an async method returning users for trainer dropdown.
            // If it is named GetTrainer() (sync) change the call accordingly.
            var users = await _serviceUser.GetTrainerAsync();
            foreach (var u in users)
            {
                list.Add(new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.UserName} - {u.EmployeeName}"
                });
            }

            return list;
        }

        // GET: Trainer
        public async Task<IActionResult> Index()
        {
            try
            {
                var trainers = await _service.GetAllAsync();
                var model = _mapper.Map<List<TrainerVM>>(trainers);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<TrainerVM>());
            }
        }

        // GET: Trainer/Create
        public async Task<IActionResult> Create()
        {
            var model = new TrainerVM();
            try
            {
                model.ListSection = new SelectList(await GetSectionAsync(), "Value", "Text");
                model.ListUser = new SelectList(await GetTrainerAsync(), "Value", "Text");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(model);
        }

        // POST: Trainer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerVM model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                // repopulate selects and return
                model.ListSection = new SelectList(await GetSectionAsync(), "Value", "Text");
                model.ListUser = new SelectList(await GetTrainerAsync(), "Value", "Text");
                return View(model);
            }

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    model.Signature = fileName;

                    var imagesFolder = Path.Combine(
                        _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                        "images");

                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    var filePath = Path.Combine(imagesFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                }

                var entity = _mapper.Map<Trainer>(model);
                await _service.InsertAsync(entity);

                TempData["SuccessMessage"] = "Trainer berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                // repopulate selects on error
                model.ListSection = new SelectList(await GetSectionAsync(), "Value", "Text");
                model.ListUser = new SelectList(await GetTrainerAsync(), "Value", "Text");
                return View(model);
            }
        }
    }
}