// Dinduction.Web/Controllers/AccountController.cs
using AutoMapper;
using Dinduction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dinduction.Web.Models;

namespace Dinduction.Web.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public AccountController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    // GET: /Account/Login
    public IActionResult Login(string returnUrl = null)
    {
        HttpContext.Session.Clear();
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userService.LoginAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "User Name is Incorrect!");
                return View(model);
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (user.StartTraining.HasValue && user.StartTraining > today)
            {
                ModelState.AddModelError("", "Training has not started yet. Please contact HR.");
                return View(model);
            }

            if (user.EndTraining.HasValue && user.EndTraining < today)
            {
                ModelState.AddModelError("", "Date Training Expired. Please contact HR.");
                return View(model);
            }

            if (user.Password != model.Password)
            {
                ModelState.AddModelError("", "Password Is Incorrect!");
                return View(model);
            }

            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("EmployeeName", user.EmployeeName ?? "");
            HttpContext.Session.SetString("Role", user.RoleId?.ToString() ?? "2");

            await _userService.UpdateAsync(user); 

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // GET: /Account/ChangePassword
    public IActionResult ChangePassword()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            return RedirectToAction("Login");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(UserVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login");

            var user = await _userService.LoginAsync(userName);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("", "Current Password Not Matched");
                return View(model);
            }

            if (model.NewPassword != model.ConfNewPassword)
            {
                ModelState.AddModelError("", "New Password and Confirmed New Password Not Matched");
                return View(model);
            }

        
            user.Password = model.NewPassword;
            await _userService.UpdateAsync(user);

            ViewBag.SuccessMessage = "Password changed successfully!";
            return View();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [AllowAnonymous]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}