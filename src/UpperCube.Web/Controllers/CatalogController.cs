using Microsoft.AspNetCore.Mvc;

namespace UpperCube.Web.Controllers;

public class CatalogController : Controller
{
    public IActionResult Index() => View();
}
