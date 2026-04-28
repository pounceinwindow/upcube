using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace UpperCube.Web.Controllers;

public class CultureController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
}
