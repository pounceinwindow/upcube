using Microsoft.AspNetCore.Mvc;

namespace UpperCube.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
