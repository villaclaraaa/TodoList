using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        this._logger = logger;
    }

    public IActionResult Index()
    {
        if (this.User.Identity.IsAuthenticated)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return this.RedirectToAction("Index", "TodoList", new { ownerId = userId });
        }

        return this.RedirectToPage("/Account/Login", new { area = "Identity" });
    }

    public IActionResult TodoList()
    {
        return this.View("TodoList/Index");
    }

    public IActionResult Privacy()
    {
        return this.View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}
