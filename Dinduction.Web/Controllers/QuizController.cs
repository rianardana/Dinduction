using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dinduction.Web.Models;

namespace Dinduction.Web.Controllers;

public class QuizController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }
}