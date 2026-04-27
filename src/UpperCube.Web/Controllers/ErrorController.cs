using Microsoft.AspNetCore.Mvc;

namespace UpperCube.Controllers;

public sealed class ErrorController : Controller
{
    [Route("error/{statusCode:int}")]
    public IActionResult StatusCodePage(int statusCode)
    {
        Response.StatusCode = statusCode;

        return statusCode switch
        {
            StatusCodes.Status404NotFound => View("Error404"),
            StatusCodes.Status403Forbidden => View("Error403"),
            _ => View("Error")
        };
    }
}