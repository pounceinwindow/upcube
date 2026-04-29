using Microsoft.AspNetCore.Mvc;

namespace UpperCube.Web.Controllers;

public class EstimatorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}